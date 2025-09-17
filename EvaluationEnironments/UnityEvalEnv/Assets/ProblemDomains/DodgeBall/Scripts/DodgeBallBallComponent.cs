using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Problems.DodgeBall
{
    public class DodgeBallBallComponent : MonoBehaviour
    {
        public Rigidbody Rigidbody { get; private set; }
        public SphereCollider SphereCollider { get; private set; }
        public Vector3 StartPosition { get; private set; }
        public Quaternion StartRotation { get; private set; }
        public DodgeBallAgentComponent Parent { get; set; }

        private DodgeBallEnvironmentController DodgeBallEnvironmentController;

        private void Awake()
        {
            Rigidbody = GetComponent<Rigidbody>();
            SphereCollider = GetComponent<SphereCollider>();
            DodgeBallEnvironmentController = GetComponentInParent<DodgeBallEnvironmentController>();
            StartPosition = transform.position;
            StartRotation = transform.rotation;
        }

        public void SetBallActive(bool isActive)
        {
            if (Rigidbody)
            {
                Rigidbody.isKinematic = !isActive;
                SphereCollider.enabled = isActive;
            }
        }

        public void ResetBall()
        {
            if (Rigidbody)
            {
                Rigidbody.velocity = Vector3.zero;
                Rigidbody.angularVelocity = Vector3.zero;
                SphereCollider.enabled = true;

                Rigidbody.position = StartPosition;
                Rigidbody.rotation = StartRotation;
            }
            SetBallActive(true);
            Parent = null;

            transform.position = StartPosition;
            transform.rotation = StartRotation;
        }

        private void OnCollisionEnter(Collision other)
        {
            if(other.gameObject.TryGetComponent<DodgeBallAgentComponent>(out DodgeBallAgentComponent agent))
            {
                if (agent != Parent && Parent)
                {
                    DodgeBallEnvironmentController.BallHitAgent(agent, this);
                }
            }
            else
            {
                Parent = null;
            }
        }
    }
}
