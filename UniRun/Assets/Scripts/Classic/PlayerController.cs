using UnityEngine;

// PlayerController는 플레이어 캐릭터로서 Player 게임 오브젝트를 제어한다.
public class PlayerController : MonoBehaviour {
   public int maxJumpCount = 2;
   public AudioClip deathClip; // 사망시 재생할 오디오 클립
   public float jumpForce = 700f; // 점프 힘

   private int jumpCount = 0; // 누적 점프 횟수
   private bool isGrounded = false; // 바닥에 닿았는지 나타냄
   private bool isDead = false; // 사망 상태

   private Rigidbody2D playerRigidbody; // 사용할 리지드바디 컴포넌트
   private Animator animator; // 사용할 애니메이터 컴포넌트
   private AudioSource playerAudio; // 사용할 오디오 소스 컴포넌트

   private void Start() {
       playerRigidbody = GetComponent<Rigidbody2D>();
       animator = GetComponent<Animator>();
       playerAudio = GetComponent<AudioSource>();
   }

   private void Update() {
       if (isDead) return;
   }

   public void OnJump(UnityEngine.InputSystem.InputAction.CallbackContext ctx) {
       if (isDead) return;
       if (ctx.performed) {
           TryJump();
       } else if (ctx.canceled && playerRigidbody.linearVelocity.y > 0) {
           playerRigidbody.linearVelocity = new Vector2(
               playerRigidbody.linearVelocity.x, playerRigidbody.linearVelocity.y * 0.5f);
       }
   }

   private void TryJump() {
       if (jumpCount >= maxJumpCount) return;
       jumpCount++;
       playerRigidbody.linearVelocity = Vector2.zero;
       playerRigidbody.AddForce(new Vector2(0, jumpForce));
       playerAudio?.Play();
       animator?.SetBool("Grounded", false);
   }

   private void Die() {
       animator.SetTrigger("Die");
       playerAudio.clip = deathClip;
       playerAudio.Play();
       playerRigidbody.linearVelocity = Vector2.zero;
       isDead = true;

       // 게임 매니저의 게임오버 처리 실행
       GameManager.instance.OnPlayerDead();
   }

   private void OnTriggerEnter2D(Collider2D other) {
       if (other.CompareTag("Dead") && !isDead) Die();
   }

   private void OnCollisionEnter2D(Collision2D collision) {
       if (collision.contactCount > 0 && collision.GetContact(0).normal.y > 0.7f) {
           isGrounded = true;
           jumpCount = 0;
           animator?.SetBool("Grounded", true);
       }
   }

   private void OnCollisionExit2D(Collision2D collision) {
       isGrounded = false;
       animator?.SetBool("Grounded", false);
   }
}
