using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CatGame
{
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

                    foreach (GameObject rootObject in unityScene.GetRootGameObjects())
                    {
                        Context.Runner.MoveToRunnerSceneExtended(rootObject);
                    }
                    SceneManager.UnloadSceneAsync(unityScene);
                    break;
                }
            }
        }
        protected override void OnTick()
        {
            if (Context.Runner != null)
            {
                //Context.Runner.SetVisible(Context.IsVisible);
            }
            base.OnTick();
        }
        protected override void CollectServices()
        {
            base.CollectServices();
            
        }

    }
}
