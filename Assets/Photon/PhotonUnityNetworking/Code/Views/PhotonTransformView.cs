// ----------------------------------------------------------------------------
// <copyright file="PhotonTransformView.cs" company="Exit Games GmbH">
//   PhotonNetwork Framework for Unity - Copyright (C) 2018 Exit Games GmbH
// </copyright>
// <summary>
//   Component to synchronize Transforms via PUN PhotonView.
// </summary>
// <author>developer@exitgames.com</author>
// ----------------------------------------------------------------------------


namespace Photon.Pun
{
    using UnityEngine;

    [AddComponentMenu("Photon Networking/Photon Transform View")]
    [HelpURL("https://doc.photonengine.com/en-us/pun/v2/gameplay/synchronization-and-state")]
    public class PhotonTransformView : MonoBehaviourPun, IPunObservable
    {
        private float m_Distance;
        private float m_Angle;

        private Vector3 m_Direction;
        private Vector3 m_NetworkPosition;
        private Vector3 m_StoredPosition;

        private Quaternion m_NetworkRotation;

        public bool m_SynchronizePosition = true;
        public bool m_SynchronizeRotation = true;
        public bool m_SynchronizeScale = false;
        public bool m_Animate = true;
        public bool m_UseWorldTransform = false;

        bool m_firstTake = false;

        public void Awake()
        {
            if (m_UseWorldTransform)
            {
                m_StoredPosition = transform.position;
            } 
            else
            {
                m_StoredPosition = transform.localPosition;
            }
            m_NetworkPosition = Vector3.zero;

            m_NetworkRotation = Quaternion.identity;
        }

        void OnEnable()
        {
            m_firstTake = true;
        }

        public void Update()
        {
            if (!this.photonView.IsMine)
            {
                if (m_Animate) 
                {
                    if (m_UseWorldTransform)
                    {
                        transform.position = Vector3.MoveTowards(transform.position, this.m_NetworkPosition, this.m_Distance * (1.0f / PhotonNetwork.SerializationRate));
                        transform.rotation = Quaternion.RotateTowards(transform.rotation, this.m_NetworkRotation, this.m_Angle * (1.0f / PhotonNetwork.SerializationRate));
                    } 
                    else
                    {
                        transform.localPosition = Vector3.MoveTowards(transform.localPosition, this.m_NetworkPosition, this.m_Distance * (1.0f / PhotonNetwork.SerializationRate));
                        transform.localRotation = Quaternion.RotateTowards(transform.localRotation, this.m_NetworkRotation, this.m_Angle * (1.0f / PhotonNetwork.SerializationRate));
                    }
                    
                } 
                else
                {
                    if (m_UseWorldTransform)
                    {
                        transform.position = this.m_NetworkPosition;
                        transform.rotation = this.m_NetworkRotation;
                    }
                    else
                    {
                        transform.localPosition = this.m_NetworkPosition;
                        transform.localRotation = this.m_NetworkRotation;
                    }
                }
            }
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                if (this.m_SynchronizePosition)
                {
                    Vector3 pos;
                    if (m_UseWorldTransform)
                    {
                        pos = transform.position;
                    }
                    else
                    {
                        pos = transform.localPosition;
                    }

                    this.m_Direction = pos - this.m_StoredPosition;
                    this.m_StoredPosition = pos;

                    stream.SendNext(pos);
                    stream.SendNext(this.m_Direction);
                }

                if (this.m_SynchronizeRotation)
                {
                    if (m_UseWorldTransform)
                    {
                        stream.SendNext(transform.rotation);
                    }
                    else
                    {
                        stream.SendNext(transform.localRotation);
                    }
                }

                if (this.m_SynchronizeScale)
                {
                    if (m_UseWorldTransform)
                    {
                        stream.SendNext(transform.lossyScale);
                    }
                    else
                    {
                        stream.SendNext(transform.localScale);
                    }
                }
            }
            else
            {


                if (this.m_SynchronizePosition)
                {
                    this.m_NetworkPosition = (Vector3)stream.ReceiveNext();
                    this.m_Direction = (Vector3)stream.ReceiveNext();

                    if (m_firstTake)
                    {
                        if (m_UseWorldTransform)
                        {
                            transform.position = this.m_NetworkPosition;
                        }
                        else
                        {
                            transform.localPosition = this.m_NetworkPosition;
                        }
                        this.m_Distance = 0f;
                    }
                    else
                    {
                        if (m_Animate)
                        {
                            float lag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));
                            this.m_NetworkPosition += this.m_Direction * lag;
                        }

                        if (m_UseWorldTransform)
                        {
                            this.m_Distance = Vector3.Distance(transform.position, this.m_NetworkPosition);
                        }
                        else
                        {
                            this.m_Distance = Vector3.Distance(transform.localPosition, this.m_NetworkPosition);
                        }
                    }
                }

                if (this.m_SynchronizeRotation)
                {
                    this.m_NetworkRotation = (Quaternion)stream.ReceiveNext();

                    if (m_firstTake)
                    {
                        this.m_Angle = 0f;
                        if (m_UseWorldTransform)
                        {
                            transform.rotation = this.m_NetworkRotation;
                        }
                        else
                        {
                            transform.localRotation = this.m_NetworkRotation;
                        }
                    }
                    else
                    {
                        if (m_UseWorldTransform)
                        {
                            this.m_Angle = Quaternion.Angle(transform.rotation, this.m_NetworkRotation);
                        }
                        else
                        {
                            this.m_Angle = Quaternion.Angle(transform.localRotation, this.m_NetworkRotation);
                        }
                    }
                }

                if (this.m_SynchronizeScale)
                {
                    transform.localScale = (Vector3)stream.ReceiveNext();
                }

                if (m_firstTake)
                {
                    m_firstTake = false;
                }
            }
        }
    }
}