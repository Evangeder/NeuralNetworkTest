using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class WalkAi : MonoBehaviour
{
    public bool             netInitalized       = false;
    public NeuralNetwork    net;

    [SerializeField] private float currentScore = float.MinValue;

    public Transform        ChestTransform,
                            HeadTransform,
                            LeftArmTransform,
                            RightArmTransform,
                            LeftLegTransform,
                            LeftFootTransform,
                            RightLegTransform,
                            RightFootTransform;

    public Rigidbody2D      Chest,
                            Head,
                            LeftArm,
                            RightArm,
                            LeftLeg,
                            LeftFoot,
                            RightLeg,
                            RightFoot;

    [SerializeField] SpriteRenderer ChestRenderer, HeadRenderer, LeftArmRenderer, RightArmRenderer, LeftLegRenderer, LeftFootRenderer, RightLegRenderer, RightFootRenderer;

    public AiManager        manager;

    private int             _shouldPunish       = 0;

    float lastPosition = 0f;

    float killTimer = 1f;
    float killTimerValue = 1f;

    public bool dontReward = false;

    public float timeAlive = 0f;

    enum Neuron
    {
        Head = 0,
        LeftArm,
        RightArm,
        LeftLeg,
        LeftFoot,
        RightLeg,
        RightFoot,
        Torso
    }

    #region messages
    void PunishBecauseOfCollision()         => _shouldPunish++;
    void StopPunishingBecauseOfCollision()  => _shouldPunish--;
    #endregion

    public void Init(NeuralNetwork net) => this.net = net;

    public void Shade()
    {
        if (wasKilled) return;

        ChestRenderer.color     = new Color32(127, 127, 127, 127);
        ChestRenderer.sortingOrder = 1;
        HeadRenderer.color      = new Color32(127, 127, 127, 127);
        HeadRenderer.sortingOrder = 0;
        LeftArmRenderer.color   = new Color32(127, 127, 127, 127);
        LeftArmRenderer.sortingOrder = 0;
        RightArmRenderer.color  = new Color32(127, 127, 127, 127);
        RightArmRenderer.sortingOrder = 0;
        LeftLegRenderer.color   = new Color32(127, 127, 127, 127);
        LeftLegRenderer.sortingOrder = 0;
        LeftFootRenderer.color  = new Color32(127, 127, 127, 127);
        LeftFootRenderer.sortingOrder = 0;
        RightLegRenderer.color  = new Color32(127, 127, 127, 127);
        RightLegRenderer.sortingOrder = 0;
        RightFootRenderer.color = new Color32(127, 127, 127, 127);
        RightFootRenderer.sortingOrder = 0;
    }
    public void MakeVisible()
    {
        if (wasKilled) return;

        ChestRenderer.color     = Color.white;
        ChestRenderer.sortingOrder = 101;
        HeadRenderer.color      = Color.white;
        HeadRenderer.sortingOrder = 101;
        LeftArmRenderer.color   = Color.white;
        LeftArmRenderer.sortingOrder = 101;
        RightArmRenderer.color  = Color.white;
        RightArmRenderer.sortingOrder = 101;
        LeftLegRenderer.color   = Color.white;
        LeftLegRenderer.sortingOrder = 101;
        LeftFootRenderer.color  = Color.white;
        LeftFootRenderer.sortingOrder = 101;
        RightLegRenderer.color  = Color.white;
        RightLegRenderer.sortingOrder = 101;
        RightFootRenderer.color = Color.white;
        RightFootRenderer.sortingOrder = 101;
    }

    bool wasKilled = false;
    public void Kill()
    {
        if (wasKilled) return;
        wasKilled = true;
        dontReward = true;
        Destroy(Chest.gameObject);
        Destroy(Head);
        Destroy(LeftArm);
        Destroy(RightArm);
        Destroy(LeftLeg);
        Destroy(LeftFoot);
        Destroy(RightLeg);
        Destroy(RightFoot);
    }

    void Update()
    {
        //if (transform.rotation.eulerAngles.z > 90 && transform.rotation.eulerAngles.z < 270) Kill();

        //if (transform.position.x < -0.2f)
        //{
        //    net.SetFitness(-999);
        //    net.SetTimeAlive(-999);
        //    Kill();
        //}

        if (dontReward) return;

        if (netInitalized && manager.Countdown > 0)
        {
            if (_shouldPunish < 0) _shouldPunish = 0;

            float[] inputs = {
                HeadTransform.rotation.z,
                LeftArmTransform.rotation.z,
                RightArmTransform.rotation.z,
                LeftLegTransform.rotation.z,
                LeftFootTransform.rotation.z,
                RightLegTransform.rotation.z,
                RightFootTransform.rotation.z,
                _shouldPunish > 0f ? 100f : 0f,
                transform.position.x,
                Chest.transform.rotation.z,
                Chest.velocity.x,
                Chest.velocity.y,
                LeftLeg.velocity.x,
                LeftLeg.velocity.y,
                LeftLeg.angularVelocity,
                LeftFoot.velocity.x,
                LeftFoot.velocity.y,
                LeftFoot.angularVelocity,
                RightLeg.velocity.x,
                RightLeg.velocity.y,
                RightLeg.angularVelocity,
                RightFoot.velocity.x,
                RightFoot.velocity.y,
                RightFoot.angularVelocity,
                Vector2.Distance(new Vector2(transform.position.x, 0), new Vector2(WoD.Pos, 0))
            };

            float[] neurons = net.FeedForward(inputs);

            //HeadTransform.Rotate(Vector3.forward         , neurons[(int)Neuron.Head]);
            //LeftArmTransform.Rotate(Vector3.forward      , neurons[(int)Neuron.LeftArm]);
            //RightArmTransform.Rotate(Vector3.forward     , neurons[(int)Neuron.RightArm]);
            //LeftLegTransform.Rotate(Vector3.forward      , neurons[(int)Neuron.LeftLeg]);
            //LeftFootTransform.Rotate(Vector3.forward     , neurons[(int)Neuron.LeftFoot]);
            //RightLegTransform.Rotate(Vector3.forward     , neurons[(int)Neuron.RightLeg]);
            //RightFootTransform.Rotate(Vector3.forward    , neurons[(int)Neuron.RightFoot]);
            //ChestTransform.Rotate(Vector3.forward        , neurons[(int)Neuron.Torso]);

            Head.AddTorque(     neurons[(int)Neuron.Head]       * 5f,  ForceMode2D.Force);
            LeftArm.AddTorque(  neurons[(int)Neuron.LeftArm]    * 5f,  ForceMode2D.Force);
            RightArm.AddTorque( neurons[(int)Neuron.RightArm]   * 5f,  ForceMode2D.Force);
            LeftLeg.AddTorque(  neurons[(int)Neuron.LeftLeg]    * 20f,  ForceMode2D.Force);
            LeftFoot.AddTorque( neurons[(int)Neuron.LeftFoot]   * 20f,  ForceMode2D.Force);
            RightLeg.AddTorque( neurons[(int)Neuron.RightLeg]   * 20f,  ForceMode2D.Force);
            RightFoot.AddTorque(neurons[(int)Neuron.RightFoot]  * 20f,  ForceMode2D.Force);
            Chest.AddTorque(    neurons[(int)Neuron.Torso]      * 5f,  ForceMode2D.Force);

            if (_shouldPunish > 0)
            {
                if (killTimerValue > 0)
                    killTimerValue -= Time.deltaTime;
                else
                    Kill();
            }
            else
            {
                net.SetFitness(transform.position.x * 2);
                killTimerValue = killTimer;
            }

            net.SetTimeAlive(timeAlive);
            currentScore = net.GetFitness();

            lastPosition = transform.position.x;

            if (WoD.Pos >= transform.position.x)
                Kill();
        }

        timeAlive += Time.deltaTime;
    }
}
