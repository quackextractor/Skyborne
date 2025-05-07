using UnityEngine;
using UnityEngine.SceneManagement;

namespace Menu
{
    public class LevelLoad : MonoBehaviour
    {
        // Start is called before the first frame update
        private void Start()
        {
        }

        // Update is called once per frame
        private void Update()
        {
        }

        public void LoadLevel()
        {
            SceneManager.LoadScene(1);
        }

        public void Terminate()
        {
            Application.Quit();
        }
    }
}