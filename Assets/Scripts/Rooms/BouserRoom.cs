using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Rooms/Bouser Room")]
    public class BouserRoom : Room
    {
        public Transform[] BouserWaypoints => bouserWaypoints;
        [SerializeField] private Transform[] bouserWaypoints;
    }
}
