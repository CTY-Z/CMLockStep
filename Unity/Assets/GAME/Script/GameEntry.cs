using LS;
using LS.Utils;
using UnityEngine;

public class GameEntry : Singleton<GameEntry>
{
    public EventPool eventPool;
    public NetworkManager network;
    public ModelManager model;
    public LSWorld world;

    public bool isCreate = false;

    public UDPClient client;
    public GameObject playerPrefab;

    private void Awake()
    {
        Application.runInBackground = true;

        eventPool = new EventPool();
        network = new NetworkManager();
        model = new ModelManager();
        world = new LSWorld();
    }

    private void Start()
    {
        network.Init();
        model.Init();

        LSLogic lsLogic = new LSLogic();
        LSView lsView = new LSView(playerPrefab);
        world.Init(lsLogic, lsView, new InputComparer());

        if (isCreate)
            Instantiate(client, this.transform);

        //Heartbeat hb = new Heartbeat();
        //hb.Str = "ping";
        //byte[] dataByte = ProtobufHelper.Encode(1, 1, hb);
        //
        //Debug.Log(dataByte);
        //
        //var meta = ProtobufHelper.DecodeHeader(dataByte);
        //Debug.Log(meta);
        //
        //Debug.Log(ProtobufHelper.DecodeData<Heartbeat>(dataByte).Str);
    }

    private void OnApplicationQuit()
    {
        world.ShutDown();
    }
}

