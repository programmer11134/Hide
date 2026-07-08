using UnityEngine;

public class LexusRaycastGun : MonoBehaviour
{
    // Ссылка на камеру, чтобы знать, откуда пускать луч
    public Camera playerCamera;

    // Максимальная дистанция выстрела
    public float fireRange = 100f;

    void Update()
    {
        // Если нажата левая кнопка мыши (0)
        if (Input.GetButtonDown("Fire1"))
        {
            Shoot();
        }
    }

    void Shoot()
    {
        RaycastHit hit;
        // Пускаем луч ровно из центра экрана вперед
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        

        if (Physics.Raycast(ray, out hit, fireRange))
        {
            // Выводим в консоль имя объекта, в который попали
            Debug.Log("Попадание в: " + hit.collider.name);

            // Простой тест: красим то, во что попали, в красный цвет
            Renderer hitRenderer = hit.collider.GetComponent<Renderer>();
            if (hitRenderer != null)
            {
                hitRenderer.material.color = Color.red;
            }
        }
        else
        {
            Debug.Log("Промах!");
        }
    }
}