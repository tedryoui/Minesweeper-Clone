using System;
using UnityEngine.AddressableAssets;
using UnityEngine.Audio;

namespace _.Scripts.Utility.Asset_References
{
    [Serializable]
    public class AssetReferenceAudioMixer : AssetReferenceT<AudioMixer>
    {
        public AssetReferenceAudioMixer(string guid) : base(guid)
        {
        }
    }
}