using System;
using UnityEngine;

public class DronePropeller : MonoBehaviour
{
    public GameObject propeller1; // ?v???y??1
    public GameObject propeller2; // ?v???y??2
    public GameObject propeller3; // ?v???y??3
    public GameObject propeller4; // ?v???y??4
    public GameObject propeller5;
    public GameObject propeller6;

    public bool enableAudio = true;
    public float maxRotationSpeed = 1f; // ???????]???x?i?x/?b?j
    private AudioSource audioSource;
    public string audio_path;
    public Camera target_camera;
    public float maxDistance = 5.0f;
    public float minDistance = 0.0f;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (enableAudio)
        {
            LoadAudio();
        }
    }

    private void LoadAudio()
    {
        AudioClip clip = Resources.Load<AudioClip>(this.audio_path);
        if (clip != null)
        {
            Debug.Log("audio found: " + audio_path);
            audioSource.clip = clip;
            audioSource.Stop();
        }
        else
        {
            Debug.LogWarning("audio not found: " + audio_path);
        }
    }

    private void PlayAudio(float my_controls)
    {
        float distance = Vector3.Distance(target_camera.transform.position, transform.position);
        float volume = 1.0f - Mathf.Clamp01((distance - minDistance) / (maxDistance - minDistance));

        if (audioSource.isPlaying == false && my_controls > 0)
        {
            audioSource.Play();
        }
        else if (audioSource.isPlaying == true && my_controls == 0)
        {
            audioSource.Stop();
        }

        if (audioSource.isPlaying)
        {
            audioSource.volume = volume;
        }
    }
    private void RotatePropeller(GameObject propeller, float dutyRate)
    {
        if (propeller == null) return;
        float rotationSpeed = maxRotationSpeed * dutyRate;
        propeller.transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f);
    }
    public void Rotate(float c1, float c2, float c3, float c4)
    {
        RotatePropeller(propeller1, c1);
        RotatePropeller(propeller2, -c2);
        RotatePropeller(propeller3, c3);
        RotatePropeller(propeller4, -c4);
        if (propeller5)
        {
            RotatePropeller(propeller5, c1);
        }
        if (propeller6)
        {
            RotatePropeller(propeller6, c2);
        }
        if (enableAudio)
        {
            PlayAudio(c1);
        }

    }
}
