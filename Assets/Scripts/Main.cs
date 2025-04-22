using System.Threading.Tasks;
using Doinject;
using Mew.Core.Assets;

using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

public class Main : MonoBehaviour, IInjectableComponent
{
    IContext Context { get; set; }
    public UnifiedScene titleSceneReference;
    //public SceneReference titleSceneReference;
    SceneContextLoader SceneContextLoader { get; set; }

    [Inject]
    public void Construct(IContext context, SceneContextLoader sceneContextLoader)
    {
        Context = context;
        SceneContextLoader = sceneContextLoader;
    }

    [OnInjected]
    public async ValueTask OnInjected()
    {
        // ParentSceneContextRequirement によって初期化された場合は、初期シーンロードを行わない
        if(Context.IsReverseLoaded) return;

        await GotoTitle();
    }
    public async ValueTask GotoTitle()
    {
        
        await SceneContextLoader.LoadAsync(titleSceneReference, active : true);
    }
}
