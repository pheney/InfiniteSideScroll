using UnityEngine;
using System.Collections;

namespace PLib.MouseInput
{
    [System.Serializable]
    public class MouseClickEvent
    {
        public string name;
        public int buttonID;
        public MouseButtonState mouseEvent;
        [Tooltip("Behaviors that will recieve mouse-click messages")]
        public MonoBehaviour[] receivers;
    }

    public class OnMouseClickSendClickMessage : MonoBehaviour
    {
        #region Inspector Assigned

        public MouseClickEvent[] mouseEvents;

        #endregion
        #region Data

        MouseClickData[] clickData;

        #endregion
        #region Unity API

        void Update()
        {
            UpdateMouseData();
            SendMessages();
        }

        #endregion
        #region Internal Methods

        private void UpdateMouseData()
        {
            Vector2 position = Input.mousePosition;
            for (int i = 0; i < 3; i++)
            {
                clickData[i].buttonID = i;
                clickData[i].mousePosition = position;
                clickData[i].buttonEvent = MouseButtonState.NONE;

                if (Input.GetMouseButtonDown(i))
                {
                    clickData[i].buttonEvent = MouseButtonState.DOWN;
                }
                if (Input.GetMouseButtonUp(i))
                {
                    clickData[i].buttonEvent = MouseButtonState.UP;
                }
                if (Input.GetMouseButton(i))
                {
                    clickData[i].buttonEvent = MouseButtonState.HOLD;
                }
            }
        }

        private void SendMessages()
        {
            foreach (var eventData in mouseEvents)
            {
                MouseClickData click = clickData[eventData.buttonID];
                if (click.buttonID != eventData.buttonID) continue;
                if (!click.buttonEvent.Equals(eventData.mouseEvent)) continue;

                foreach (MonoBehaviour b in eventData.receivers)
                {
                    b.SendMessage(Message.INPUT_MOUSE_BUTTON, click, SendMessageOptions.DontRequireReceiver);
                }
            }
        }

        #endregion
    }
}