using UnityEngine;
using UnityEngine.Events;
using static AnarPerPortes.ShortUtils;

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
        [SerializeField] private Transform hardmodeLaser;
        [SerializeField][Min(0f)] private float hardmodeTimeToKill = 10f;

        private float hardmodeTimer;
        private bool hardmodeTimerActive = true;

        public void SetSignMaterials(Material correctWayMaterial, Material incorrectWayMaterial)
        {
            correctWaySign.material = correctWayMaterial;
            incorrectWaySign.material = incorrectWayMaterial;
        }

        public override void Initialize()
        {
            door.OnDoorOpened.AddListener(() => incorrectDoor.Deactivate());
            incorrectDoor.OnDoorOpened.AddListener(() => OnIncorrectDoorOpened?.Invoke());

            if (IsHardmodeEnabled())
            {
                door.OnDoorOpened.AddListener(DeactivateHardmodeTimer);
                incorrectDoor.OnDoorOpened.AddListener(DeactivateHardmodeTimer);
            }

            hardmodeLaser.gameObject.SetActive(IsHardmodeEnabled());

            base.Initialize();

            if (PlayerController.Singleton.HasItem("WalkieTalkie"))
                walkieTalkieGiver.SetActive(false);
        }

        public void CloseIncorrectDoor()
        {
            incorrectDoor.Close();
        }

        private void DeactivateHardmodeTimer()
        {
            hardmodeTimerActive = false;
            hardmodeLaser.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (!IsHardmodeEnabled() || !hardmodeTimerActive)
                return;

            if (!A90Enemy.IsOperative && !SheepyEnemy.IsOperative && !SangotEnemy.IsOperative)
                hardmodeTimer += Time.deltaTime;

            if (hardmodeTimer >= hardmodeTimeToKill)
            {
                hardmodeTimer = 0f;
                PlayerController.Singleton.BlockAll();
                CatchManager.Singleton.CatchPlayer("A TOMAR POR CULO", "Yusuf te ha pegao un tiro porque has tardado la vida en elegir.");
            }
        }

        private void LateUpdate()
        {
            if (!hardmodeTimerActive)
                return;

            hardmodeLaser.LookAt(PlayerController.Singleton.transform.position + new Vector3(0f, 1.0f, 0f));
        }
    }
}
