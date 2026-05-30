using System.Threading.Tasks;
using _.Scripts.Scriptable_Objects;
using _.Scripts.Services;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace _.Scripts.Entry_Point
{
    public class RootEntryPoint : IPostInitializable
    {
        [Inject] private ProjectSettings _projectSettings;
        [Inject] private VolumeService   _volumeService;
        
        public void PostInitialize()
        {
            CreateAudioSourceAsync();
        }

        private async Task CreateAudioSourceAsync()
        {
            var assetReference = _projectSettings.VolumeSourceAssetReference;

            var result = await assetReference.InstantiateAsync().Task;
            
            Object.DontDestroyOnLoad(result.gameObject);
            
            if (result.gameObject.TryGetComponent(out AudioSource audioSource))
                _volumeService.audioSource = audioSource;
        }
    }
}