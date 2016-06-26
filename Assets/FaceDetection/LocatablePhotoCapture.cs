using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.VR.WSA.WebCam;
using System.Linq;

[System.Serializable]
public class FaceRectangle : System.Object
{
    int top;
    int left;
    int height;
    int width;
}

[System.Serializable]
public class FaceId : System.Object
{
    string faceId;
    FaceRectangle faceRectangle;
}

public class LocatablePhotoCapture : MonoBehaviour
{

    public static bool faceDetected = false;
    PhotoCapture photoCaptureObj = null;
    Material mat;
    string filename;
    string filePath;

    public delegate void FaceDetectedHandler();
    public FaceDetectedHandler OnFaceDetected;


    // Use this for initialization
    void Start()
    {

        filename = string.Format("CapturedImage.jpg");
        filePath = System.IO.Path.Combine(Application.persistentDataPath, filename);
        mat = gameObject.GetComponent<Renderer>().material;
        PhotoCapture.CreateAsync(false, OnPhotoCaptureCreated);

        OnFaceDetected += OnFaceDetectedHandler;
    }

    void OnPhotoCaptureCreated(PhotoCapture captureObject)
    {
        photoCaptureObj = captureObject;

        Resolution cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();

        CameraParameters c = new CameraParameters();
        c.hologramOpacity = 0f;
        c.cameraResolutionWidth = cameraResolution.width;
        c.cameraResolutionHeight = cameraResolution.height;
        c.pixelFormat = CapturePixelFormat.BGRA32;

        captureObject.StartPhotoModeAsync(c, false, OnPhotoModeStarted);
    }

    void OnStoppedPhotoMode(PhotoCapture.PhotoCaptureResult result)
    {
        photoCaptureObj.Dispose();
        photoCaptureObj = null;
    }

    private void OnPhotoModeStarted(PhotoCapture.PhotoCaptureResult result)
    {
        if (result.success)
        {
            Debug.Log("Started Photo Mode");
            //StartCoroutine(TakePhoto());

            StartCoroutine(TakePhoto());
        }

        else
        {
            Debug.LogError("Unable to start photo mode");
        }
    }

    public void changeColour()
    {
        mat.SetColor("_Color", new Color(1f, 0f, 0f));
    }

    IEnumerator TakePhoto()
    {
        while (!faceDetected)
        {
            //photoCaptureObj.TakePhotoAsync(filePath, PhotoCaptureFileOutputFormat.JPG, OnCapturedPhotoToDisk);
            photoCaptureObj.TakePhotoAsync(OnCapturedPhotoToMemory);
            yield return new WaitForSeconds(1f);
        }
    }

    // Save to Tex2d
    void OnCapturedPhotoToMemory(PhotoCapture.PhotoCaptureResult result, PhotoCaptureFrame photoCaptureFrame)
    {
        if (result.success)
        {
            Resolution cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();
            Texture2D targetTexture = new Texture2D(cameraResolution.width, cameraResolution.height);
            photoCaptureFrame.UploadImageDataToTexture(targetTexture);
            photoCaptureFrame.Dispose();
            Destroy(mat.mainTexture);
            mat.SetTexture("_MainTex", targetTexture);
            MakeDetectRequest(targetTexture);
        }
        //photoCaptureObj.StopPhotoModeAsync(OnStoppedPhotoMode);
    }

    void OnCapturedPhotoToDisk(PhotoCapture.PhotoCaptureResult result)
    {
        if (result.success)
        {
            Debug.Log("Saved Photo to disk!");
            //face.DetectFace(filePath);
            //CallDetectFace.MakeRequest(filePath);
        }
        else
        {
            Debug.Log("Failed to save Photo to disk");
        }
    }

    void OnApplicationQuit()
    {
        photoCaptureObj.StopPhotoModeAsync(OnStoppedPhotoMode);
    }

    public void MakeDetectRequest(Texture2D tex)
    {
        StartCoroutine(UploadAndDetect(tex));
    }

    IEnumerator UploadAndDetect(Texture2D tex)
    {
        WWWForm form = new WWWForm();
        string url = "https://api.projectoxford.ai/face/v1.0/detect";
        byte[] bytes = tex.EncodeToPNG();
        Dictionary<string, string> headers = form.headers;

        headers["Ocp-Apim-Subscription-Key"] = "d1c36bdc6d7f4eb292369a9b9ec5caaf";
        headers["Content-Type"] = "application/octet-stream";
        headers["Accept"] = "application/json";


        WWW w = new WWW(url, bytes, headers);
        yield return w;
        if (!string.IsNullOrEmpty(w.error))
        {
            Debug.Log(w.error);
        }
        else
        {
            // if returned "[]", no faces were detected;
            if (w.text == "[]")
            {
                // do nothing
            }
            else
            {
                // parse face info
                try
                {
                    FaceId f = new FaceId();
                    string t = w.text.TrimStart('[').TrimEnd(']');
                    f = JsonUtility.FromJson<FaceId>(t);
                    faceDetected = true;
                    Debug.Log("FaceDetected");
                    if (OnFaceDetected != null)
                    {
                        OnFaceDetected();
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError(e.Message);
                }
            }

        }
    }

    void OnFaceDetectedHandler()
    {
        Debug.Log("Face Detected");
    }
}
