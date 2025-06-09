using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UI;

public static class PlayerControls
{
    private static PlayerChicken chicken;
    private static Controls controls;

    public static void Initialize(PlayerChicken owner)
    {
        //bind our owner
        chicken = owner;
        //create the controls object
        controls = new Controls();
        //when the dash button is pressed, read the value as a button (truth or down, false for up)
        controls.Game.Dash.performed += context => owner.SetDashState(context.ReadValueAsButton());
        controls.Game.Cluck.performed += context => owner.SetCluckState(context.ReadValueAsButton());
        controls.Game.Jump.performed += context => owner.SetJumpState(context.ReadValueAsButton());

        //read the value as a type of vector 2
        controls.Game.Move.performed += context => owner.SetMoveDirection(context.ReadValue<Vector2>());
        controls.Game.Look.performed += context => owner.SetLookDirection(context.ReadValue<Vector2>());

        //the underscore means this discard (we don't care about the input)
        controls.Game.EnableUI.performed += _ =>
        {
           Settings.OpenSettings(false);
            UseUIControls();
        };

        controls.UI.DisableUI.performed += _ =>
        {
           Settings.CloseSettings();
            UseGameControls();
        };
    }

    public static void UseGameControls()
    {
        //enable game, disable ui
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        controls.Game.Enable();
        controls.UI.Disable();
    }

    public static void UseUIControls()
    {
        //disable the player in the game, and enable the ui
        DisablePlayer();
        controls.Game.Disable();
        controls.UI.Enable();
    }

    public static void DisablePlayer()
    {
        //disable all controls
        controls.UI.Disable();
        controls.Game.Disable();
        /* If we disable the controls, Unity will no longer check to see when we stop our input.
         Therefore, we need to send a message to all of our inputs as if we have let go all of them.
        If we need to disable the player.
         */
        chicken.SetCluckState(false);
        chicken.SetDashState(false);
        chicken.SetJumpState(false);
        chicken.SetLookDirection(Vector2.zero);
        chicken.SetMoveDirection(Vector3.zero);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

}
