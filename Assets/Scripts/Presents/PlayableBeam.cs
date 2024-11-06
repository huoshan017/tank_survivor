using Logic.Component;
using UnityEngine;

public class PlayableBeam : PlayableProjectile
{
    protected new void Start()
    {
        base.Start();
        CheckAndCreateRayRenderer();
    }

    protected new void Update()
    {
        rayRenderer_.SetPositions(linePoints_);
    }

    protected override void OnShootingStartHandle()
    {
        CheckAndCreateRayRenderer();
        gameObject.SetActive(true);
        EnableStartPointParticles(true);
        EnableEndPointParticles(true);
    }

    protected override void OnShootingStopHandle()
    {
        rayRenderer_.positionCount = 0;
        gameObject.SetActive(false);
        EnableStartPointParticles(false);
        EnableEndPointParticles(false);
    }

    protected override void OnShootingHitHandle(ShootingHitInfo shootingInfo)
    {
        CheckAndCreateRayRenderer();
        var logicPos = transformComp_.Pos;
        transform.position = new Vector3(logicPos.X(), 0, logicPos.Y()) / GlobalConstant.LogicAndUnityRatio;
        var hitPos = shootingInfo.Pos;
        linePoints_[0] = transform.position;
        linePoints_[1] = new Vector3(hitPos.X(), 0, hitPos.Y()) / GlobalConstant.LogicAndUnityRatio;
        rayRenderer_.positionCount = 2;
        CheckStartPointParticles();
        CheckEndPointParticles();

        var degree = shootingInfo.Dir.Degree() + (float)shootingInfo.Dir.Minute() / 60;
        var eulerAngles = new Vector3(0, degree, 0);
        startPointParticles_[0].transform.eulerAngles = eulerAngles;
        for (int i = 0; i < endPointParticles_.Length; i++)
        {
            endPointParticles_[i].transform.position = linePoints_[1];
        }
        if (shootingInfo.IsHit)
        {
            endPointParticles_[0].Play();
            endPointParticles_[0].transform.eulerAngles = eulerAngles;
        }
        else
        {
            endPointParticles_[0].Stop();
        }
    }

    void CheckAndCreateRayRenderer()
    {
        if (rayRenderer_ == null)
        {
            rayRenderer_ = gameObject.GetComponent<LineRenderer>();
            rayRenderer_.positionCount = 2;
            linePoints_ = new Vector3[2];
        }
    }

    void EnableStartPointParticles(bool enable)
    {
        CheckStartPointParticles();
        if (enable)
        {
            for (int i = 0; i < startPointParticles_.Length; i++)
            {
                startPointParticles_[i].Play();
            }
        }
        else
        {
            for (int i = 0; i < startPointParticles_.Length; i++)
            {
                startPointParticles_[i].Stop();
            }
        }
    }

    void EnableEndPointParticles(bool enable)
    {
        CheckEndPointParticles();
        if (enable)
        {
            endPointParticles_[1].Play();
        }
        else
        {
            for (int i = 0; i < endPointParticles_.Length; i++)
            {
                endPointParticles_[i].Stop();
            }
        }
    }

    void CheckStartPointParticles()
    {
        if (startPointParticles_ == null)
        {
            var sp = transform.Find("StartParticles");
            startPointParticles_ = new ParticleSystem[sp.childCount];
            for (int i = 0; i < sp.childCount; i++)
            {
                var spc = sp.GetChild(i);
                var ps = spc.GetComponent<ParticleSystem>();
                startPointParticles_[i] = ps;
            }
        }
    }

    void CheckEndPointParticles()
    {
        if (endPointParticles_ == null)
        {
            var sp = transform.Find("EndParticles");
            endPointParticles_ = new ParticleSystem[sp.childCount];
            for (int i = 0; i < sp.childCount; i++)
            {
                var spc = sp.GetChild(i);
                var ps = spc.GetComponent<ParticleSystem>();
                endPointParticles_[i] = ps;
            }
        }
    }

    LineRenderer rayRenderer_;
    Vector3[] linePoints_;
    ParticleSystem[] startPointParticles_;
    ParticleSystem[] endPointParticles_;
}