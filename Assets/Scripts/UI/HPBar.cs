using UnityEngine;

public class HPBar : MonoBehaviour
{
  public static float ZHeight = -1000;
  public Transform healthBar;
  public Transform backgroundBar;
  private Transform cameraToFace;

  protected void Start()
  {
    cameraToFace = Camera.main.transform;
  }

  protected void Update()
  {
    if (cameraToFace != null)
    {
      Vector3 direction = cameraToFace.transform.forward;
      transform.forward = -direction;
    }
  }

  public void UpdateHealth(float normalizedHealth)
  {
    Vector3 scale = Vector3.one;

    if (healthBar != null)
    {
      scale.x = normalizedHealth;
      healthBar.transform.localScale = scale;
    }

    if (backgroundBar != null)
    {
      scale.x = 1 - normalizedHealth;
      backgroundBar.transform.localScale = scale;
    }

    SetVisible(normalizedHealth < 1.0f);
  }

  public void SetVisible(bool visible)
  {
    gameObject.SetActive(visible);
  }
}

