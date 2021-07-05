using Ruinarch;
namespace Player_Input {
    /// <summary>
    /// Base class for actions that wait for the Players Input.
    /// This is a component added to the InputManager
    /// <see cref="PlayerManager._playerInputModules"/>
    /// </summary>
    public abstract class PlayerInputModule {

        public abstract void OnUpdate();
    }
}