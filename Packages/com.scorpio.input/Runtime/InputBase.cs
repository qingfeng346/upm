using UnityEngine;
namespace Scorpio.Input {
    public interface InputBase {
        bool Touch(out Vector2 position, out Vector2 deltePosition, out int fingerId);
        bool TouchZoom(ref float rate);
    }
}