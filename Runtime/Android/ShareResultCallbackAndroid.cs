using UnityEngine;

namespace Conkist.Tools
{
    ﻿#if UNITY_EDITOR || UNITY_ANDROID

    namespace ShareNamespace
    {
    	public class ShareResultCallbackAndroid : AndroidJavaProxy
    	{
    		private readonly CallbackHelper callbackHelper;

    		public ShareResultCallbackAndroid( Share.ShareResultCallback callback ) : base( "com.yasirkula.unity.ShareResultReceiver" )
    		{
    			callbackHelper = new GameObject( "CallbackHelper" ).AddComponent<CallbackHelper>();
    			callbackHelper.callback = callback;
    		}

    		public void OnShareCompleted( int result, string shareTarget )
    		{
    			if( !callbackHelper )
    			{
    				Debug.LogWarning( "CallbackHelper is destroyed!" );
    				return;
    			}

    			callbackHelper.OnShareCompleted( result, shareTarget );
    		}

    		public bool HasManagedCallback()
    		{
    			return callbackHelper && callbackHelper.callback != null;
    		}
    	}
    }
    #endif
}
