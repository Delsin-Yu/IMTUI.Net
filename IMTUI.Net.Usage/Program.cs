using IMTUI;
using IMTUI.Nodes;

var ui = new MyUI();
ImmediateModeUIRenderer.Run(ui);
Console.ReadKey();
return;

internal class MyUI : IImmediateModeTerminalUI
{
    public void OnDraw(TerminalUIInstance tui)
    {
        using var _ = tui.VBox();
        tui.Label("Lorem ipsum dolor sit amet sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt explicabo.", ConsoleColor.Cyan);
        tui.Label("Nemo enim ipsam voluptatem quia voluptas sit aspernatur aut odit aut fugit, sed quia consequuntur magni dolores eos qui ratione voluptatem sequi nesciunt. ", ConsoleColor.Yellow);
        tui.Label("Neque porro quisquam est, qui dolorem ipsum quia dolor sit amet, consectetur. Ut enim ad minima veniam, quis nostrum exercitationem");
        tui.Label("ullam corporis suscipit laboriosam, nisi ut aliquid ex ea commodi consequatur?  ", ConsoleColor.Red);
        tui.Label("Quis autem vel eum iure reprehenderit qui in ea voluptate velit esse quam nihil molestiae consequatur, vel illum qui dolorem eum fugiat quo voluptas nulla pariatur? At vero eos et accusamus et iusto odio dignissimos ducimus qui blanditiis praesentium voluptatum deleniti atque corrupti quos dolores et quas molestias excepturi sint occaecati cupiditate non provident.  ", ConsoleColor.Blue);
    }
}


