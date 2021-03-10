using UnityEngine;

namespace Assets.FirstPersion_AIO_Pack.DemoScene.MiscScripts {
    public class Doors : MonoBehaviour {
        void OnTriggerEnter(Collider coll)
        {
            if (coll.tag == "Player") {
                GetComponent<Animator>().Play("Door_open");
                this.enabled = false;
            }
        }
    }
}