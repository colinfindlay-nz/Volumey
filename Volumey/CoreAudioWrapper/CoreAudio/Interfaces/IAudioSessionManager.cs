﻿using System;
using System.Runtime.InteropServices;
using Volumey.CoreAudioWrapper.Wrapper;

namespace Volumey.CoreAudioWrapper.CoreAudio.Interfaces
{
    [Guid(GuidValue.External.IAudioSessionManager), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IAudioSessionManager
    {
        /// <summary>
        /// Return an audio session control for the current process.
        /// </summary>
        /// <param name="sessionGuid"></param>
        /// <param name="streamFlags"></param>
        /// <param name="sessionControl"></param>
        /// <returns></returns>
        int GetAudioSessionControl(ref Guid sessionGuid, bool streamFlags, out IAudioSessionControl sessionControl);

        /// <summary>
        /// Return an audio volume control for the current process.
        /// </summary>
        /// <param name="sessionGuid">Pointer to a session GUID. 
        /// If this parameter is NULL or points to the value GUID_NULL, the method assigns the stream to the default session.</param>
        /// <param name="streamFlags">Specifies whether the request is for a cross-process session. 
        /// Set to TRUE if the session is cross-process. Set to FALSE if the session is not cross-process.</param>
        /// <param name="audioVolume"></param>
        
        [PreserveSig]
        int GetSimpleAudioVolume(ref Guid sessionGuid, bool streamFlags, out ISimpleAudioVolume audioVolume);
    }
}
