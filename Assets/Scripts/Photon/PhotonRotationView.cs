using Photon.Pun;
using UnityEngine;

public class PhotonRotationView : MonoBehaviourPun, IPunObservable
{
    private float m_Distance;
    private float m_Angle;

    private Quaternion m_NetworkRotation;

    public bool m_SynchronizeRotation = true;

    [Tooltip(
        "Indicates if localPosition and localRotation should be used. Scale ignores this setting, and always uses localScale to avoid issues with lossyScale.")]
    public bool m_UseLocal;

    bool m_firstTake = false;

    public void Awake()
    {
        m_NetworkRotation = Quaternion.identity;
    }

    private void Reset()
    {
        m_UseLocal = true;
    }

    void OnEnable()
    {
        m_firstTake = true;
    }

    public void Update()
    {
        var tr = transform;

        if (!this.photonView.IsMine)
        {
            if (m_UseLocal)

            {
                tr.localRotation = Quaternion.RotateTowards(tr.localRotation, this.m_NetworkRotation,
                    this.m_Angle);
            }
            else
            {
                tr.rotation = Quaternion.RotateTowards(tr.rotation, this.m_NetworkRotation, this.m_Angle);
            }
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        var tr = transform;
        if (stream.IsWriting)
        {
            if (this.m_SynchronizeRotation)
            {
                if (m_UseLocal)
                {
                    stream.SendNext(tr.localRotation);
                }
                else
                {
                    stream.SendNext(tr.rotation);
                }
            }
        }
        else
        {
            if (this.m_SynchronizeRotation)
            {
                this.m_NetworkRotation = (Quaternion)stream.ReceiveNext();

                if (m_firstTake)
                {
                    this.m_Angle = 0f;

                    if (m_UseLocal)
                    {
                        tr.localRotation = this.m_NetworkRotation;
                    }
                    else
                    {
                        tr.rotation = this.m_NetworkRotation;
                    }
                }
                else
                {
                    if (m_UseLocal)
                    {
                        this.m_Angle = Quaternion.Angle(tr.localRotation, this.m_NetworkRotation);
                    }
                    else
                    {
                        this.m_Angle = Quaternion.Angle(tr.rotation, this.m_NetworkRotation);
                    }
                }
            }

            if (m_firstTake)
            {
                m_firstTake = false;
            }
        }
    }
}