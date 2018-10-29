namespace WebSharperSPA

open WebSharper
open WebSharper.JavaScript
open WebSharper.JQuery
open WebSharper.UI
open WebSharper.UI.Html
open WebSharper.UI.Client
open WebSharper.UI.Templating
open System

[<JavaScript>]
module Client =

    type IndexTemplate = Template<"index.html", ClientLoad.FromDocument>

    [<NoComparison>]
    type Person = {
        Name : string
        Created : DateTime }

    let People =
        ListModel.Create (fun person -> person.Name)
            [
                { Name = "John"; Created = DateTime.Now.AddDays(-2.0) }
                { Name = "Paul"; Created = DateTime.Now.AddDays(-12.0) }
            ]

    [<SPAEntryPoint>]
    let Main () =
        let isNameAlreadyUsed s =
            People.ContainsKey s

        let isNameInvalid (s : string) =
            String.IsNullOrEmpty s //|| s.Length < 3

        let newName = Var.Create ""
        let nameAlreadyUsed = newName.View.Map isNameAlreadyUsed
        let nameInvalid = newName.View.Map isNameInvalid
        let checkboxStatus = Var.Create false

        IndexTemplate.Main()
            .ListContainer(
                People.View.DocSeqCached (fun (person : Person) ->
                    IndexTemplate.ListItem()
                        .Name(person.Name)
                        .Created(person.Created.ToShortDateString())
                        .Delete(fun _ -> People.RemoveByKey person.Name)
                        .Doc()
                )
            )
            .Name(newName)
            .AttrNameMessage(Attr.ClassPred "hidden" (not nameAlreadyUsed.V))
            .AttrTick(Attr.ClassPred "hidden" (nameInvalid.V || nameAlreadyUsed.V))
            .AttrCross(Attr.ClassPred "hidden" (not (nameInvalid.V || nameAlreadyUsed.V)))
            .AttrAdd(Attr.DynamicProp "disabled" nameInvalid)
            .Add(fun e ->
                if not (isNameInvalid e.Vars.Name.Value) then
                    People.Add { Name = newName.Value; Created = DateTime.Now }
                    newName.Value <- ""
            )
            .Checkbox(checkboxStatus)
            .CheckboxStatus(checkboxStatus.View.Map string)
            .Doc()
        |> Doc.RunById "main"
