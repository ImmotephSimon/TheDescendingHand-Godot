[GlobalClass]
public partial class ItemIconView : Control
{
    [Export] private TextureRect image;

    public void Render(Texture2D icon, Vector2 position, Vector2 size)
    {
        Position = position;
        Size = size;
        if (image != null) image.Texture = icon;
    }
}