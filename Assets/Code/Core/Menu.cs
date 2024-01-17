using System.Collections;

namespace CatGame
{
    public class Menu : Base
    {
        protected override IEnumerator OnActivate()
        {
            yield return base.OnActivate();

            if (ApplicationSettings.IsQuickPlay == true)
            {
                //juegos rapidos configurar
            }
        }
    }
}
