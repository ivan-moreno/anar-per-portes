using UnityEngine;
using UnityEngine.Events;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Rooms/Isle Room")]
    public class IsleRoom : Room
    {
        public RoomDoor IncorrectDoor => incorrectDoor;
        public UnityEvent OnIncorrectDoorOpened { get; } = new();
        [SerializeField] private RoomDoor incorrectDoor;
        [SerializeField] private GameObject walkieTalkieGiver;
        [SerializeField] private Renderer correctWaySign;
        [SerializeField] private Renderer incorrectWaySign;

        public void SetSignMaterials(Material correctWayMaterial, Material incorrectWayMaterial)
        {
            correctWaySign.material = correctWayMaterial;
            incorrectWaySign.material = incorrectWayMaterial;
        }

        public override void Initialize()
        {
            door.OnDoorOpened.AddListener(() => incorrectDoor.Deactivate());
            incorrectDoor.OnDoorOpened.AddListener(() => OnIncorrectDoorOpened?.Invoke());
            base.Initialize();

            if (PlayerController.Singleton.HasItem("WalkieTalkie"))
                walkieTalkieGiver.SetActive(false);
        }

        public void CloseIncorrectDoor()
        {
            incorrectDoor.Close();
        }
    }
}
