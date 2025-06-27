using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MMG.UI
{
    public class PopupBase : MonoBehaviour
    {
        public virtual void Open()
        {
            gameObject.SetActive(true);
            GetComponent<Animator>()?.Play("Open");
        }

        public virtual void Close()
        {
            GetComponent<Animator>()?.Play("Close");
            StartCoroutine(DestroyAfterDelay());
        }

        private IEnumerator DestroyAfterDelay()
        {
            yield return new WaitForSeconds(0.5f);
            Destroy(gameObject);
        }
    }
}