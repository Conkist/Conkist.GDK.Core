using System.Collections;
using UnityEngine;

namespace Conkist.Tools
{
    #if UNITY_EDITOR || UNITY_ANDROID

    namespace ShareNamespace
    {
    	public class CallbackHelper : MonoBehaviour
    	{
    		public Share.ShareResultCallback callback;

    		private Share.ShareResult result = Share.ShareResult.Unknown;
    		private string shareTarget = null;

    		private bool resultReceived;

    		private void Awake()
    		{
    			DontDestroyOnLoad( gameObject );
    		}

    		private void Update()
    		{
    			if( resultReceived )
    			{
    				resultReceived = false;

    				try
    				{
    					if( callback != null )
    						callback( result, shareTarget );
    				}
    				finally
    				{
    					Destroy( gameObject );
    				}
    			}
    		}

    		private IEnumerator OnApplicationFocus( bool focus )
    		{
    			if( focus )
    			{
    				// Share sheet is closed and now Unity activity is running again. Send Unknown result if OnShareCompleted wasn't called
    				yield return null;
    				resultReceived = true;
    			}
    		}

    		public void OnShareCompleted( int resultRaw, string shareTarget )
    		{
    			Share.ShareResult shareResult = (Share.ShareResult) resultRaw;

    			if( result == Share.ShareResult.Unknown )
    			{
    				result = shareResult;
    				this.shareTarget = shareTarget;
    			}
    			else if( result == Share.ShareResult.NotShared )
    			{
    				if( shareResult == Share.ShareResult.Shared )
    				{
    					result = Share.ShareResult.Shared;
    					this.shareTarget = shareTarget;
    				}
    				else if( shareResult == Share.ShareResult.NotShared && !string.IsNullOrEmpty( shareTarget ) )
    					this.shareTarget = shareTarget;
    			}
    			else
    			{
    				if( shareResult == Share.ShareResult.Shared && !string.IsNullOrEmpty( shareTarget ) )
    					this.shareTarget = shareTarget;
    			}

    			resultReceived = true;
    		}
    	}
    }
    #endif
}
