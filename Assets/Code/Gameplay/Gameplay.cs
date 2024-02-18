namespace CatGame
{
    using System.Collections;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    public class Gameplay : Base
    {
        private const string UI_SCENE_NAME = "GameplayUI";
        private Scene _UIScene;
        protected override void OnInitialize()
        {
            base.OnInitialize();
            var contextBehaviours = Context.Runner.SimulationUnityScene.GetComponents<IContextBehaviour>(true);
            foreach (var behaviour in contextBehaviours) 
            {
                behaviour.Context = Context;
            }
        }
        protected override IEnumerator OnActivate()
        {
            yield return base.OnActivate();
            var asyncOP = SceneManager.LoadSceneAsync(UI_SCENE_NAME,LoadSceneMode.Additive);
            while (asyncOP.isDone == false) 
                yield return null;
            for (int i = SceneManager.sceneCount; i-->0;)
            {
                var unityScene = SceneManager.GetSceneAt(i);
                if (unityScene.name == UI_SCENE_NAME)
                {
                    _UIScene = unityScene;
                    var uiService = _UIScene.GetComponent<UI.BaseUI>(true);
                    foreach (GameObject rootObject in unityScene.GetRootGameObjects())
                    {
                        Context.Runner.MoveToRunnerSceneExtended(rootObject);
                    }
                    Context.UI = uiService;
                    SceneManager.UnloadSceneAsync(unityScene);
                    AddService(uiService);
                    uiService.Activate();
                    break;
                }
            }
        }
        protected override void OnTick()
        {
            if (Context.Runner != null)
            {
                Context.Runner.IsVisible = Context.IsVisible;
            }
            base.OnTick();
        }
        protected override void CollectServices()
        {
            base.CollectServices();
            
        }

    }
}
