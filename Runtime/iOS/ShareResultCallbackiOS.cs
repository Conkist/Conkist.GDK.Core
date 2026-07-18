using UnityEngine;

namespace Conkist.Tools
{
    ﻿#if UNITY_EDITOR || UNITY_IOS

    namespace ShareNamespace
    {
    	public class ShareResultCallbackiOS : MonoBehaviour
    	{
    		private static ShareResultCallbackiOS instance;
    		private Share.ShareResultCallback callback;

    		public static void Initialize( Share.ShareResultCallback callback )
    		{
    			if( instance == null )
    			{
    				instance = new GameObject( "ShareResultCallbackiOS" ).AddComponent<ShareResultCallbackiOS>();
    				DontDestroyOnLoad( instance.gameObject );
    			}
    			else if( instance.callback != null )
    				instance.callback( Share.ShareResult.Unknown, null );

    			instance.callback = callback;
    		}

    		public void OnShareCompleted( string message )
    		{
    			Share.ShareResultCallback _callback = callback;
    			callback = null;

    			if( _callback != null )
    			{
    				if( string.IsNullOrEmpty( message ) )
    					_callback( Share.ShareResult.Unknown, null );
    				else
    				{
    					Share.ShareResult result = (Share.ShareResult) ( message[0] - '0' ); // Convert first char to digit and then to ShareResult
    					string shareTarget = message.Length > 1 ? message.Substring( 1 ) : null;

    					_callback( result, shareTarget );
    				}
    			}
    		}
    	}
    }
    #endif
}
