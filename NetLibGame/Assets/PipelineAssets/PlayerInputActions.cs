// GENERATED AUTOMATICALLY FROM 'Assets/PipelineAssets/PlayerInputActions.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class @PlayerInputActions : IInputActionCollection, IDisposable
{
    public InputActionAsset asset { get; }
    public @PlayerInputActions()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""PlayerInputActions"",
    ""maps"": [
        {
            ""name"": ""MenuNavigation"",
            ""id"": ""c14e8baf-d18c-4159-818a-8882eb19af65"",
            ""actions"": [
                {
                    ""name"": ""Enter"",
                    ""type"": ""Button"",
                    ""id"": ""c77a2bcc-8214-45e3-b2d1-7b36214d49a0"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""8fc10738-95c4-481b-a0c0-c28604e3f1a8"",
                    ""path"": ""<Keyboard>/enter"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Enter"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // MenuNavigation
        m_MenuNavigation = asset.FindActionMap("MenuNavigation", throwIfNotFound: true);
        m_MenuNavigation_Enter = m_MenuNavigation.FindAction("Enter", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    // MenuNavigation
    private readonly InputActionMap m_MenuNavigation;
    private IMenuNavigationActions m_MenuNavigationActionsCallbackInterface;
    private readonly InputAction m_MenuNavigation_Enter;
    public struct MenuNavigationActions
    {
        private @PlayerInputActions m_Wrapper;
        public MenuNavigationActions(@PlayerInputActions wrapper) { m_Wrapper = wrapper; }
        public InputAction @Enter => m_Wrapper.m_MenuNavigation_Enter;
        public InputActionMap Get() { return m_Wrapper.m_MenuNavigation; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(MenuNavigationActions set) { return set.Get(); }
        public void SetCallbacks(IMenuNavigationActions instance)
        {
            if (m_Wrapper.m_MenuNavigationActionsCallbackInterface != null)
            {
                @Enter.started -= m_Wrapper.m_MenuNavigationActionsCallbackInterface.OnEnter;
                @Enter.performed -= m_Wrapper.m_MenuNavigationActionsCallbackInterface.OnEnter;
                @Enter.canceled -= m_Wrapper.m_MenuNavigationActionsCallbackInterface.OnEnter;
            }
            m_Wrapper.m_MenuNavigationActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Enter.started += instance.OnEnter;
                @Enter.performed += instance.OnEnter;
                @Enter.canceled += instance.OnEnter;
            }
        }
    }
    public MenuNavigationActions @MenuNavigation => new MenuNavigationActions(this);
    public interface IMenuNavigationActions
    {
        void OnEnter(InputAction.CallbackContext context);
    }
}
