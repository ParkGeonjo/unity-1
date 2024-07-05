using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

/*
 * CSVWriter�� EyeGazeObserver�� ���� ���������� ���� �����͸� ������ CSV ���Ϸ� �����ϴ� ������ �����մϴ�.
 * CSVWriter takes data from serveral observers and store it as CSV file.
 */
public class CSVWriter : MonoBehaviour
{
    [SerializeField]
    private bool isEnabledRecording = true; // ���̸� CSV ����, �����̸� ���� ����.  True - save csv file, False - don't save

    [SerializeField]
    private EyeGazeObserver eyeGazeObserver;

    [SerializeField]
    private HeadObserver headObserver;

    [SerializeField]
    private FaceObserver faceObserver;

    [SerializeField]
    private HandObserver handObserver;
    [SerializeField]
    private VoiceObserver voiceObserver;
        [SerializeField]
    private bool isRecording = false; // ���� ����������� ���� ��/���� ��. True/False value for indicating whether being recording.

    private string foldername; // CSV�� ����� ���� �̸�.  folder name where csv is saved.
    private string filename; // CSV ���� �̸�.    csv file name

    private List<string> colnames = new List<string> { }; // csv�� ������ �� �̸�. csv column names
    private List<string[]> csvData = new List<string[]>(); // csv data
    private List<string> rowData = new List<string>(); // ���� ������ ���������� �����͵��� ��� �ӽ� ����Ʈ.  Temporary list to store current data from observers.
    
    private float timer = 0f; // CSV ���� ù �࿡ ǥ�õ� Ÿ�̸�.  Timer that appears in first column of CSV file.
    private bool isDesktop = true;

    private void Start()
    {
        if(SystemInfo.deviceType.ToString().Contains("Desktop"))
            isDesktop=true;
        else
            isDesktop=false;
        // ����� Ȱ��ȭ�� ��쿡�� �ʱ�ȭ ����.  Initialize only if recording is enabled.
        if (isEnabledRecording)
        {
            foldername = DateTime.Now.ToString("yyyy-MM-dd-HH-ss\\hmm\\m");
            Initialize("");
        }   
    }

    public void Initialize(string filename)
    {
        // �� Task���� ���ο� CSV ������ �����Ǿ�� �ϹǷ� �ʱ�ȭ �� ������ csv�� ����� ����Ʈ���� ����ݴϴ�.
        // New csv file is created each task so clear the list each initialization process.
        colnames.Clear();
        csvData.Clear();

        this.filename = filename.EndsWith(".csv") ? filename : filename + ".csv"; // ���ϸ��� '.csv'�� ���Ե��� ���� ��� �ڿ� '.csv' �߰�.    Add '.csv' to the end of file name if '.csv' is not included in original filename.

        colnames.Add("timer"); // add timer column

        // �� �������� �÷��� �߰�
        // Add colum names from each observers.
        foreach (string colname in eyeGazeObserver.GetColumnNames())
            colnames.Add(colname);
        foreach (string colname in headObserver.GetColumnNames())
            colnames.Add(colname);
        foreach (string colname in faceObserver.GetColumnNames())
            colnames.Add(colname);
        foreach (string colname in handObserver.GetColumnNames())
            colnames.Add(colname);
        foreach (string colname in voiceObserver.GetColumnNames())
            colnames.Add(colname);
        

        csvData.Add(colnames.ToArray()); // column name
        isRecording = true;
    }

    // Update�� �ƴ� FixedUpdate�� �̿��ϴ� ������ ����Ʈ ������ ������ ���� ���ø� �ӵ��� ���� Update()�� �ξ� ���� ������ �ߺ��� �߻��� �� �ֱ� ����.
    // The reason why FixedUpdate() is using to store sensor data is Update() speed is faster than sensor sampling speed in Quest Pro so data duplication problem is occured.
    private void FixedUpdate()
    {
        timer += Time.deltaTime;
        // Task�� �����ϰ� �ִ� �߿��� �����͸� ������.
        // Store data only the task is running.
        if (isRecording)
        {
            // ����, rowData ����Ʈ�� �������鿡 �����͸� ��������, ����Ʈ�� �迭�� ��ȯ�Ͽ� csvData�� ����.
            // First, collect data from observers to rowData list and convert rowData list to array and add to csvData.
            rowData.Clear();
            rowData.Add(timer.ToString());
            rowData.AddRange(eyeGazeObserver.GetCSVData()); // eye gaze
            rowData.AddRange(headObserver.GetCSVData()); // head
            rowData.AddRange(faceObserver.GetCSVData()); // face
            rowData.AddRange(handObserver.GetCSVData()); // hand
            rowData.AddRange(voiceObserver.GetCSVData());
            csvData.Add(rowData.ToArray());
        }
    }

    // ���α׷��� ����Ǵ� ��� �ڵ����� CSV ������ ������.
    // Save the csv file on program quit.
    private void OnApplicationQuit()
    {
        Save();
    }

    // CSV ���� ����.
    // Save CSV file.
    public void Save()
    {
        if (!isEnabledRecording)
            return;

        string[][] output = new string[csvData.Count][];
        for (int i = 0; i < output.Length; i++)
            output[i] = csvData[i];

        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < output.GetLength(0); i++)
            sb.AppendLine(string.Join(",", output[i]));
        string filePath = Application.persistentDataPath + "/" + foldername;
        //if (!Directory.Exists(filePath))
        //    Directory.CreateDirectory(filePath);

        StreamWriter outStream = System.IO.File.CreateText(filePath + filename);
        outStream.Write(sb);
        outStream.Close();
        isRecording = false;
    }

    // OS ���� �⺻ ���� ��ġ�� �ٸ��Ƿ� �̸� ���Ͻ�Ŵ.
    // Each operation system has different store path so make same path to save csv file.
    private string GetPath()
    {
        string path = null;
        switch (Application.platform)
        {
            case RuntimePlatform.Android:
                path = Application.persistentDataPath;
                return path.Substring(0, path.LastIndexOf('/'));
            case RuntimePlatform.IPhonePlayer:
            case RuntimePlatform.OSXEditor:
            case RuntimePlatform.OSXPlayer:
                path = Application.persistentDataPath;
                return path = path.Substring(0, path.LastIndexOf('/'));
            case RuntimePlatform.WindowsEditor:
                path = Application.dataPath;
                return path = path.Substring(0, path.LastIndexOf('/'));
            default:
                path = Application.dataPath;
                return path.Substring(0, path.LastIndexOf('/'));
        }
    }
}
