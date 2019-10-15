﻿using System;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using EType = HOTK_TrackedDevice.EType;

public class HOTK_TrackedDeviceManager : MonoBehaviour
{
    public static Action<EType, uint> OnControllerIndexChanged; // Called any time a specific controller changes index
    public static Action OnControllerIndicesUpdated; // Called only when both controllers have been checked/assigned or are swapped

    public readonly Dictionary<HOTK_TrackedDevice, TrackedDeviceControllerStates> ControllerStates = new Dictionary<HOTK_TrackedDevice, TrackedDeviceControllerStates>();

    public static HOTK_TrackedDeviceManager Instance
    {
        get { return _instance ?? (_instance = new GameObject("HOTK_TrackedDeviceManager", typeof(HOTK_TrackedDeviceManager)) { hideFlags = HideFlags.HideInHierarchy }.GetComponent<HOTK_TrackedDeviceManager>()); }
    }

    public uint LeftIndex
    {
        get
        {
            return _leftIndex;
        }
    }
    public uint RightIndex
    {
        get
        {
            return _rightIndex;
        }
    }

    public uint ThirdIndex
    {
        get
        {
            return _thirdIndex;
        }
    }
    public uint FourthIndex
    {
        get
        {
            return _fourthIndex;
        }
    }
    public uint HMDIndex
    {
        get
        {
            return _hmdIndex;
        }
    }

    private static HOTK_TrackedDeviceManager _instance;

    private uint _leftIndex = OpenVR.k_unTrackedDeviceIndexInvalid;
    private uint _rightIndex = OpenVR.k_unTrackedDeviceIndexInvalid;
    private uint _thirdIndex = OpenVR.k_unTrackedDeviceIndexInvalid;
    private uint _fourthIndex = OpenVR.k_unTrackedDeviceIndexInvalid;
    private uint _hmdIndex = OpenVR.k_unTrackedDeviceIndexInvalid;

    public void Awake()
    {
        FindHMD();
        FindControllers();
        FindExtraControllers();
    }

    public void Start()
    {
        StartCheckingForControllers();
    }

    public void Update()
    {
        FindHMD();
        FindControllers();
        FindExtraControllers();
        UpdatePoses();
    }

    private readonly TrackedDevicePose_t[] _poses = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];
    private readonly TrackedDevicePose_t[] _gamePoses = new TrackedDevicePose_t[0];

    /// <summary>
    /// Grab the last poses from the compositor and push them to the event listeners.
    /// This method should be disabled if SteamVR_Render.cs is being used, because it is also called there.
    /// </summary>
    private void UpdatePoses()
    {
        var compositor = OpenVR.Compositor;
        if (compositor == null) return;
        compositor.GetLastPoses(_poses, _gamePoses);
        SteamVR_Utils.Event.Send("new_poses", _poses);
        SteamVR_Utils.Event.Send("new_poses_applied");
    }

    /// <summary>
    /// Tests if a given Device just had a given button pressed
    /// </summary>
    /// <param name="device">The Device to test against</param>
    /// <param name="buttonId">The Button to test</param>
    /// <returns>Returns True if the given Button was just pressed, False otherwise</returns>
    public bool GetPressDown(HOTK_TrackedDevice device, EVRButtonId buttonId)
    {
        if (device == null || !device.IsValid) return false;
        var svr = SteamVR.instance;
        if (svr == null) return false;
        var c = new VRControllerState_t();
        svr.hmd.GetControllerState((uint)device.Index, ref c);

        TrackedDeviceControllerStates state;

        if (!ControllerStates.TryGetValue(device, out state))
        {
            state = new TrackedDeviceControllerStates();
            ControllerStates.Add(device, state);
        }

        bool curState;

        if (!state.PressedStates.TryGetValue(buttonId, out curState))
        {
            curState = false;
            state.PressedStates.Add(buttonId, false);
        }

        if (GetPress(c, buttonId))
        {
            if (!curState)
            {
                state.PressedStates.Remove(buttonId);
                state.PressedStates.Add(buttonId, true);
                return true;
            }
        }
        else
        {
            if (curState)
            {
                state.PressedStates.Remove(buttonId);
                state.PressedStates.Add(buttonId, false);
                return false;
            }
        }
        return false;
    }

    /// <summary>
    /// Tests if a given Device just had a given button released
    /// </summary>
    /// <param name="device">The Device to test against</param>
    /// <param name="buttonId">The Button to test</param>
    /// <returns>Returns True if the given Button was just released, False otherwise</returns>
    public bool GetPressUp(HOTK_TrackedDevice device, EVRButtonId buttonId)
    {
        if (device == null || !device.IsValid) return false;
        var svr = SteamVR.instance;
        if (svr == null) return false;
        var c = new VRControllerState_t();
        svr.hmd.GetControllerState((uint)device.Index, ref c);

        TrackedDeviceControllerStates state;

        if (!ControllerStates.TryGetValue(device, out state))
        {
            state = new TrackedDeviceControllerStates();
            ControllerStates.Add(device, state);
        }

        bool curState;

        if (!state.ReleasedStates.TryGetValue(buttonId, out curState))
        {
            curState = false;
            state.ReleasedStates.Add(buttonId, false);
        }

        if (!curState)
        {
            if (GetPress(c, buttonId))
            {
                state.ReleasedStates.Remove(buttonId);
                state.ReleasedStates.Add(buttonId, true);
                return false;
            }
        }
        else
        {
            if (!GetPress(c, buttonId))
            {
                state.ReleasedStates.Remove(buttonId);
                state.ReleasedStates.Add(buttonId, false);
                return true;
            }
        }
        return false;
    }


    /// <summary>
    /// Tests if a given Device just had a given button is being held down
    /// </summary>
    /// <param name="device">The Device to test against</param>
    /// <param name="buttonId">The Button to test</param>
    /// <returns>Returns True if the given Button is being held down, False otherwise</returns>
    public bool GetPress(HOTK_TrackedDevice device, EVRButtonId buttonId)
    {
        if (device == null || !device.IsValid) return false;
        var svr = SteamVR.instance;
        if (svr == null) return false;
        var c = new VRControllerState_t();
        svr.hmd.GetControllerState((uint)device.Index, ref c);

        return GetPress(c, buttonId);
    }

    /// <summary>
    /// Tests if a given VRControllerState_t has a button held down
    /// </summary>
    public bool GetPress(VRControllerState_t state, EVRButtonId buttonId) { return (state.ulButtonPressed & (1ul << (int)buttonId)) != 0; }

    /// <summary>
    /// Attempt to find the HMD.
    /// </summary>
    public void FindHMD()
    {
        var system = OpenVR.System;
        if (system == null)
        {
            LogError("OpenVR System not found.");
            if (!Application.isEditor) System.Diagnostics.Process.GetCurrentProcess().Kill();
            return;
        }

        if (_hmdIndex != OpenVR.k_unTrackedDeviceIndexInvalid &&
            system.GetTrackedDeviceClass(_hmdIndex) == ETrackedDeviceClass.HMD)
        {
            // Assume we as still connected to the HMD..
            return;
        }

        for (uint i = 0; i < OpenVR.k_unMaxTrackedDeviceCount; i++)
        {
            if (system.GetTrackedDeviceClass(i) != ETrackedDeviceClass.HMD) continue;
            _hmdIndex = i;
            break;
        }
        if (_hmdIndex != OpenVR.k_unTrackedDeviceIndexInvalid)
        {
            Log("Found HMD ( Device: {0} )", _hmdIndex);
        }
    }

    public void ResetControllerFindAttemptCount()
    {
        _noControllersCount = 0;
    }

    public void ResetExtraControllerFindAttemptCount()
    {
        _noExtraControllersCount = 0;
    }

    /// <summary>
    /// Check for new controllers now
    /// </summary>
    public void CheckForControllersOnce()
    {
        Log("Checking once for new controllers!");
        CheckForControllers();
    }

    /// <summary>
    /// Check for new controllers once, without reporting
    /// </summary>
    private void CheckForControllers()
    {
        if (_noControllersCount >= 10)
        {
            _noControllersCount -= 1;
        }
        if (_noExtraControllersCount >= 10)
        {
            _noExtraControllersCount -= 1;
        }
    }

    /// <summary>
    /// Stop checking for new controllers automatically
    /// </summary>
    public void StopCheckingForControllers()
    {
        Log("No longer checking for new controllers!");
        CancelInvoke("CheckForControllers");
    }

    /// <summary>
    /// Start checking for new controllers automatically.
    /// </summary>
    /// <param name="interval">How long to wait between checking for new controllers?</param>
    /// <param name="checkImmediately">Should we start checking for new controllers immediately? Otherwise we start in [interval] seconds.</param>
    public void StartCheckingForControllers(float interval = 10f, bool checkImmediately = true)
    {
        Log("Check for new controllers every {0} seconds!", interval);
        CancelInvoke("CheckForControllers");
        InvokeRepeating("CheckForControllers", checkImmediately ? 0f : interval, interval);
    }

    private bool _couldntFindControllers;
    private bool _couldntFindExtraControllers;
    private uint _noControllersCount;
    private uint _noExtraControllersCount;

    public void FindExtraControllers()
    {
        if (_noExtraControllersCount >= 10)
        {
            return;
        }
        var system = OpenVR.System;
        if (system == null)
        {
            LogError("OpenVR System not found.");
            return;
        }
        if (_thirdIndex != OpenVR.k_unTrackedDeviceIndexInvalid && system.GetTrackedDeviceClass(_thirdIndex) == ETrackedDeviceClass.Controller &&
            _fourthIndex != OpenVR.k_unTrackedDeviceIndexInvalid && system.GetTrackedDeviceClass(_fourthIndex) == ETrackedDeviceClass.Controller)
        {
            // Assume we are still connected to the extra controllers..
            return;
        }
        if (_noExtraControllersCount == 0) Log("Searching for Extra Controllers..");

        var found = 0;

        // Attempt to assign Third and Fourth controllers
        for (uint i = 0; i < OpenVR.k_unMaxTrackedDeviceCount; i++)
        {
            if (i == _leftIndex || i == _rightIndex || i == _thirdIndex || i == _fourthIndex) continue; // If this index is already assigned, skip it
            if (system.GetTrackedDeviceClass(i) != ETrackedDeviceClass.Controller) continue; // If this device is not a controller, skip it

            if (_thirdIndex == OpenVR.k_unTrackedDeviceIndexInvalid)
            {
                _thirdIndex = i;
                found++;
                Log("Found Controller ( Device: {0} ): Third", _thirdIndex);
                CallIndexChanged(EType.ThirdController, _thirdIndex);
                continue;
            }
            if (_fourthIndex == OpenVR.k_unTrackedDeviceIndexInvalid)
            {
                _fourthIndex = i;
                found++;
                Log("Found Controller ( Device: {0} ): Fourth", _fourthIndex);
                CallIndexChanged(EType.FourthController, _fourthIndex);
                continue;
            }
            if (found >= 2) // We are only expecting to find two extra controllers (right now), if we've found them, break out of this loop
                break;
        }
        if (found > 0)
        {
            Log(found > 1 ? "Found Extra Controllers!" : "Found Extra Controller!");
        }
        else
        {
            _noExtraControllersCount++;

            if (!_couldntFindExtraControllers && _noExtraControllersCount >= 10)
            {
                _couldntFindExtraControllers = true;
                Log("No Extra Controllers were found.");
            }
        }
    }

    /// <summary>
    /// Attempt to find both controllers.
    /// </summary>
    public void FindControllers()
    {
        if (_noControllersCount >= 10)
        {
            return;
        }
        var system = OpenVR.System;
        if (system == null)
        {
            LogError("OpenVR System not found.");
            return;
        }

        if (_leftIndex != OpenVR.k_unTrackedDeviceIndexInvalid && system.GetTrackedDeviceClass(_leftIndex) == ETrackedDeviceClass.Controller &&
            _rightIndex != OpenVR.k_unTrackedDeviceIndexInvalid && system.GetTrackedDeviceClass(_rightIndex) == ETrackedDeviceClass.Controller)
        {
            // Assume we are still connected to the controllers..
            return;
        }

        if (_noControllersCount == 0) Log("Searching for Controllers..");
        _leftIndex = system.GetTrackedDeviceIndexForControllerRole(ETrackedControllerRole.LeftHand);
        CallIndexChanged(EType.LeftController, _leftIndex);
        _rightIndex = system.GetTrackedDeviceIndexForControllerRole(ETrackedControllerRole.RightHand);
        CallIndexChanged(EType.RightController, _rightIndex);
        CallControllersUpdated();

        if (_leftIndex != OpenVR.k_unTrackedDeviceIndexInvalid && _rightIndex != OpenVR.k_unTrackedDeviceIndexInvalid) // Both controllers are assigned!
        {
            Log("Found Controller ( Device: {0} ): Right", _rightIndex);
            Log("Found Controller ( Device: {0} ): Left", _leftIndex);
        }
        else if (_leftIndex != OpenVR.k_unTrackedDeviceIndexInvalid && _rightIndex == OpenVR.k_unTrackedDeviceIndexInvalid) // Left controller is assigned but right is missing
        {
            Log("Found Controller ( Device: {0} ): Left", _leftIndex);
            for (uint i = 0; i < OpenVR.k_unMaxTrackedDeviceCount; i++)
            {
                if (i == _leftIndex || system.GetTrackedDeviceClass(i) != ETrackedDeviceClass.Controller)
                {
                    continue;
                }
                _rightIndex = i;
                CallIndexChanged(EType.RightController, _rightIndex);
                Log("Found Controller ( Device: {0} ): Right", _rightIndex);
                break;
            }
            CallControllersUpdated();
        }
        else if (_leftIndex == OpenVR.k_unTrackedDeviceIndexInvalid && _rightIndex != OpenVR.k_unTrackedDeviceIndexInvalid) // Right controller is assigned but left is missing
        {
            Log("Found Controller ( Device: {0} ): Right", _rightIndex);
            for (uint i = 0; i < OpenVR.k_unMaxTrackedDeviceCount; i++)
            {
                if (i == _rightIndex || system.GetTrackedDeviceClass(i) != ETrackedDeviceClass.Controller)
                {
                    continue;
                }
                _leftIndex = i;
                CallIndexChanged(EType.LeftController, _leftIndex);
                Log("Found Controller ( Device: {0} ): Left", _leftIndex);
                break;
            }
            CallControllersUpdated();
        }
        else if (_leftIndex == OpenVR.k_unTrackedDeviceIndexInvalid && _rightIndex == OpenVR.k_unTrackedDeviceIndexInvalid) // Both controllers are unassigned
        {
            if (_noControllersCount == 0) LogWarning("SteamVR Reports No Assigned Controllers..! Searching..");
            var foundUnassigned = 0;
            var slots = new uint[2];
            // Sort through all the devices until we find two controllers
            for (uint i = 0; i < OpenVR.k_unMaxTrackedDeviceCount; i++)
            {
                if (system.GetTrackedDeviceClass(i) != ETrackedDeviceClass.Controller)
                {
                    continue; // This device isn't a controller, skip it
                }
                switch (system.GetControllerRoleForTrackedDeviceIndex(i))
                {
                    case ETrackedControllerRole.LeftHand:
                        _leftIndex = i;
                        Log("Found Controller ( Device: {0} ): Left", _leftIndex);
                        CallIndexChanged(EType.LeftController, _leftIndex);
                        break;
                    case ETrackedControllerRole.RightHand:
                        _rightIndex = i;
                        Log("Found Controller ( Device: {0} ): Right", _rightIndex);
                        CallIndexChanged(EType.RightController, _rightIndex);
                        break;
                    case ETrackedControllerRole.Invalid:
                        Log("Found Controller ( Device: {0} ): Unassigned", i);
                        if (foundUnassigned <= 1)
                            slots[foundUnassigned++] = i;
                        break;
                }

                if (foundUnassigned == 2)
                {
                    break; // We have two controllers, stop searching
                }
            }
            switch (foundUnassigned)
            {
                case 2:
                    LogWarning("Found Two Unassigned Controllers! Randomly Assigning!");
                    _rightIndex = slots[0];
                    CallIndexChanged(EType.RightController, _rightIndex);
                    _leftIndex = slots[1];
                    CallIndexChanged(EType.LeftController, _leftIndex);
                    break;
                case 1:
                    if (_leftIndex == OpenVR.k_unTrackedDeviceIndexInvalid &&
                       _rightIndex != OpenVR.k_unTrackedDeviceIndexInvalid)
                    {
                        LogWarning("Only Found One Unassigned Controller, and Right was already assigned! Assigning To Left!");
                        _leftIndex = slots[0];
                        CallIndexChanged(EType.LeftController, _leftIndex);
                        _noControllersCount = 10;
                    }
                    else
                    {
                        LogWarning("Only Found One Unassigned Controller! Assigning To Right!");
                        _rightIndex = slots[0];
                        CallIndexChanged(EType.RightController, _rightIndex);
                        _noControllersCount = 10;
                    }
                    break;
                case 0:
                    if (_noControllersCount == 0) LogWarning("Couldn't Find Any Unassigned Controllers!");
                    _noControllersCount++;

                    if (!_couldntFindControllers && _noControllersCount >= 10)
                    {
                        _couldntFindControllers = true;
                        LogError("Controllers not found!");
                        LogError("Please connect the controllers and restart!");
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            CallControllersUpdated();
        }
    }

    private void CallIndexChanged(EType role, uint index)
    {
        if (OnControllerIndexChanged != null)
            OnControllerIndexChanged(role, index);
    }
    private void CallControllersUpdated()
    {
        if (OnControllerIndicesUpdated != null)
            OnControllerIndicesUpdated();
    }

    public void SwapControllers()
    {
        var t = _leftIndex;
        _leftIndex = _rightIndex;
        CallIndexChanged(EType.LeftController, _leftIndex);
        _rightIndex = t;
        CallIndexChanged(EType.RightController, _rightIndex);
        CallControllersUpdated();
    }

    /// <summary>
    /// This is just used to quickly enable/disable Log messages.
    /// </summary>
    /// <param name="text"></param>
    void Log(string text, params object[] vars)
    {
        //Debug.Log(vars == null ? text : string.Format(text, vars));
    }

    /// <summary>
    /// This is just used to quickly enable/disable LogWarning messages.
    /// </summary>
    /// <param name="text"></param>
    void LogWarning(string text, params object[] vars)
    {
        Debug.LogWarning(vars == null ? text : string.Format(text, vars));
    }

    /// <summary>
    /// This is just used to quickly enable/disable LogError messages.
    /// </summary>
    /// <param name="text"></param>
    void LogError(string text, params object[] vars)
    {
        Debug.LogError(vars == null ? text : string.Format(text, vars));
    }
}

/// <summary>
/// Used to track Controller Button States
/// </summary>
public class TrackedDeviceControllerStates
{
    public readonly Dictionary<EVRButtonId, bool> PressedStates = new Dictionary<EVRButtonId, bool>();
    public readonly Dictionary<EVRButtonId, bool> ReleasedStates = new Dictionary<EVRButtonId, bool>();
}