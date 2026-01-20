using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AllIn1SpriteShader
{
    public class All1DemoUrpCamMove : MonoBehaviour
    {
        [SerializeField] private float speed = 5;
        private Vector2 input = Vector3.zero;
        private Rigidbody2D rb;

        public void Start()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        public void FixedUpdate()
        {
            input.x = AllIn1InputSystem.GetMouseXAxis();
            input.y = AllIn1InputSystem.GetMouseYAxis();

#if UNITY_6000_0_OR_NEWER
			rb.linearVelocity = input * speed * Time.fixedDeltaTime;
#else
			rb.velocity = input * speed * Time.fixedDeltaTime;
#endif
		}
	}
}