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

    // The templates are loaded from the DOM, so you just can edit index.html
    // and refresh your browser, no need to recompile unless you add or remove holes.
    type IndexTemplate = Template<"index.html", ClientLoad.FromDocument>

    //let People = ListModel.FromSeq [ "John"; "Paul" ]

    [<NoComparison>]
    type Person = { Name : string }

    let People =
        ListModel.Create (fun person -> person.Name)
            [
                { Name = "John" }
                { Name = "Paul" }
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
                //ListModel.View People |> Doc.BindSeqCached (fun person ->
                    IndexTemplate.ListItem()
                        .Name(person.Name)
                        .Delete(fun _ -> People.RemoveByKey person.Name)
                        .Doc()
                )
            )
            .Name(newName)
            .AttrNameMessage(Attr.ClassPred "hidden" (not nameAlreadyUsed.V))
            .AttrTick(Attr.ClassPred "hidden" (nameInvalid.V || nameAlreadyUsed.V))
            .AttrCross(Attr.ClassPred "hidden" (not nameInvalid.V && not nameAlreadyUsed.V))
            .AttrAdd(Attr.DynamicProp "disabled" nameInvalid)
            .Add(fun e ->
                if not (isNameInvalid e.Vars.Name.Value) then
                    People.Add { Name = newName.Value }
                    newName.Value <- ""
            )
            .Checkbox(checkboxStatus)
            .CheckboxStatus(checkboxStatus.View.Map string)
            .Doc()
        |> Doc.RunById "main"
