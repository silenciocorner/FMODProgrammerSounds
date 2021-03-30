﻿using System;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

class ProgrammerSounds : MonoBehaviour
{
    FMOD.Studio.EVENT_CALLBACK dialogueCallback;

    [FMODUnity.EventRef]
    public string fmodEvent;

    void Start()
    {
        // Explicitly create the delegate object and assign it to a member so it doesn't get freed
        // by the garbage collected while it's being used
        dialogueCallback = new FMOD.Studio.EVENT_CALLBACK(DialogueEventCallback);
    }

    void PlayDialogue(string key)
    {
        var dialogueInstance = FMODUnity.RuntimeManager.CreateInstance(fmodEvent);

        // Pin the key string in memory and pass a pointer through the user data
        GCHandle stringHandle = GCHandle.Alloc(key, GCHandleType.Pinned);
        dialogueInstance.setUserData(GCHandle.ToIntPtr(stringHandle));

        dialogueInstance.setCallback(dialogueCallback);
        dialogueInstance.start();
        dialogueInstance.release();
    }

    [AOT.MonoPInvokeCallback(typeof(FMOD.Studio.EVENT_CALLBACK))]
    static FMOD.RESULT DialogueEventCallback(FMOD.Studio.EVENT_CALLBACK_TYPE type, IntPtr instancePtr, IntPtr parameterPtr)
    {
        FMOD.Studio.EventInstance instance = new FMOD.Studio.EventInstance(instancePtr);

        // Retrieve the user data
        IntPtr stringPtr;
        instance.getUserData(out stringPtr);

        // Get the string object
        GCHandle stringHandle = GCHandle.FromIntPtr(stringPtr);
        String key = stringHandle.Target as String;

        switch (type)
        {
            case FMOD.Studio.EVENT_CALLBACK_TYPE.CREATE_PROGRAMMER_SOUND:
                {
                    FMOD.MODE soundMode = FMOD.MODE.LOOP_NORMAL | FMOD.MODE.CREATECOMPRESSEDSAMPLE | FMOD.MODE.NONBLOCKING;
                    var parameter = (FMOD.Studio.PROGRAMMER_SOUND_PROPERTIES)Marshal.PtrToStructure(parameterPtr, typeof(FMOD.Studio.PROGRAMMER_SOUND_PROPERTIES));

                    if (key.Contains("."))
                    {
                        FMOD.Sound dialogueSound;
                        var soundResult = FMODUnity.RuntimeManager.CoreSystem.createSound(Application.streamingAssetsPath + "/" + key, soundMode, out dialogueSound);
                        if (soundResult == FMOD.RESULT.OK)
                        {
                            parameter.sound = dialogueSound.handle;
                            parameter.subsoundIndex = -1;
                            Marshal.StructureToPtr(parameter, parameterPtr, false);
                        }
                    }
                    else
                    {
                        FMOD.Studio.SOUND_INFO dialogueSoundInfo;
                        var keyResult = FMODUnity.RuntimeManager.StudioSystem.getSoundInfo(key, out dialogueSoundInfo);
                        if (keyResult != FMOD.RESULT.OK)
                        {
                            break;
                        }
                        FMOD.Sound dialogueSound;
                        var soundResult = FMODUnity.RuntimeManager.CoreSystem.createSound(dialogueSoundInfo.name_or_data, soundMode | dialogueSoundInfo.mode, ref dialogueSoundInfo.exinfo, out dialogueSound);
                        if (soundResult == FMOD.RESULT.OK)
                        {
                            parameter.sound = dialogueSound.handle;
                            parameter.subsoundIndex = dialogueSoundInfo.subsoundindex;
                            Marshal.StructureToPtr(parameter, parameterPtr, false);
                        }
                    }
                    break;
                }
            case FMOD.Studio.EVENT_CALLBACK_TYPE.DESTROY_PROGRAMMER_SOUND:
                {
                    var parameter = (FMOD.Studio.PROGRAMMER_SOUND_PROPERTIES)Marshal.PtrToStructure(parameterPtr, typeof(FMOD.Studio.PROGRAMMER_SOUND_PROPERTIES));
                    var sound = new FMOD.Sound(parameter.sound);
                    sound.release();

                    break;
                }
            case FMOD.Studio.EVENT_CALLBACK_TYPE.DESTROYED:
                {
                    // Now the event has been destroyed, unpin the string memory so it can be garbage collected
                    stringHandle.Free();

                    break;
                }
        }
        return FMOD.RESULT.OK;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            PlayDialogue("dialogue-001");
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            PlayDialogue("dialogue-002");
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            PlayDialogue("dialogue-003");
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            PlayDialogue("dialogue-004");
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            PlayDialogue("dialogue-005");
        }
    }
}


