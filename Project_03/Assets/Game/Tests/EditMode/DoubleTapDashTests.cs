using NUnit.Framework;
namespace EW.Tests
{
    public class DoubleTapDashTests
    {
        [TestCase(-1)] [TestCase(1)]
        public void SameDirectionRequiresReleaseAndFiresOnce(int dir)
        {
            var d = new DoubleTapDash();
            Assert.That(d.Poll(0,dir<0,dir>0,false,false,true,.25f),Is.Zero);
            Assert.That(d.Poll(.04f,dir<0,dir>0,false,false,true,.25f),Is.Zero,"repeat without release");
            Assert.That(d.Poll(.08f,false,false,dir<0,dir>0,true,.25f),Is.Zero);
            Assert.That(d.Poll(.16f,dir<0,dir>0,false,false,true,.25f),Is.EqualTo(dir));
            Assert.That(d.Poll(.18f,false,false,false,false,true,.25f),Is.Zero,"hold cannot repeat dash");
        }
        [Test] public void OppositeDirectionIsNotDoubleTap()
        {
            var d = new DoubleTapDash();
            d.Poll(0,true,false,false,false,true,.25f);
            d.Poll(.05f,false,false,true,false,true,.25f);
            Assert.That(d.Poll(.1f,false,true,false,false,true,.25f),Is.Zero);
        }
        [Test] public void ExpiredTapAndResetCannotDash()
        {
            var d = new DoubleTapDash();
            d.Poll(0,true,false,false,false,true,.25f);
            d.Poll(.1f,false,false,true,false,true,.25f);
            Assert.That(d.Poll(.3f,true,false,false,false,true,.25f),Is.Zero);
            d.Poll(.32f,false,false,true,false,true,.25f);d.Reset();
            Assert.That(d.Poll(.4f,true,false,false,false,true,.25f),Is.Zero);
        }
        [Test] public void BlockedCooldownDropsInputInsteadOfBuffering()
        {
            var d = new DoubleTapDash();
            d.Poll(0,true,false,false,false,true,.25f);
            d.Poll(.02f,false,false,true,false,true,.25f);
            Assert.That(d.Poll(.04f,true,false,false,false,false,.25f),Is.Zero);
            d.Poll(.06f,false,false,true,false,false,.25f);
            Assert.That(d.Poll(.08f,true,false,false,false,true,.25f),Is.Zero);
        }
        [Test] public void FirstTapDuringCooldownCountsButSecondMustBeReady()
        {
            var d = new DoubleTapDash();
            // 첫 탭은 대기 중(fire=false)에도 기록된다
            Assert.That(d.Poll(0,false,true,false,false,true,false,.25f),Is.Zero);
            d.Poll(.05f,false,false,false,true,true,false,.25f);
            // 두 번째 탭 순간 발동 가능 → 바로 대시 (한 번 더 기다리지 않는다)
            Assert.That(d.Poll(.12f,false,true,false,false,true,true,.25f),Is.EqualTo(1));
        }
        [Test] public void SecondTapWhileNotReadyIsNotBufferedForLater()
        {
            var d = new DoubleTapDash();
            d.Poll(0,true,false,false,false,true,false,.25f);
            d.Poll(.04f,false,false,true,false,true,false,.25f);
            Assert.That(d.Poll(.08f,true,false,false,false,true,false,.25f),Is.Zero,"not ready: no dash");
            // 준비된 뒤 아무것도 누르지 않으면 늦게 발동하지 않는다
            Assert.That(d.Poll(.2f,false,false,false,false,true,true,.25f),Is.Zero);
            Assert.That(d.Poll(.3f,false,false,true,false,true,true,.25f),Is.Zero);
            // AAA: 세 번째 누름은 새 첫 탭일 뿐
            Assert.That(d.Poll(.6f,true,false,false,false,true,true,.25f),Is.Zero);
        }
        [Test] public void TripleTapDoesNotChainSecondDash()
        {
            var d = new DoubleTapDash();
            d.Poll(0,true,false,false,false,true,true,.25f); d.Poll(.04f,false,false,true,false,true,true,.25f);
            Assert.That(d.Poll(.08f,true,false,false,false,true,true,.25f),Is.EqualTo(-1));
            d.Poll(.11f,false,false,true,false,true,false,.25f);
            Assert.That(d.Poll(.14f,true,false,false,false,true,true,.25f),Is.Zero,"3rd press only starts a new pair");
        }
        [Test] public void RecordFalseClearsHistory()
        {
            var d = new DoubleTapDash();
            d.Poll(0,false,true,false,false,true,true,.25f); d.Poll(.04f,false,false,false,true,true,true,.25f);
            d.Poll(.06f,false,false,false,false,false,false,.25f);
            Assert.That(d.Poll(.1f,false,true,false,false,true,true,.25f),Is.Zero);
        }
        [Test] public void DodgeIsDerivedFromMovementKeysAndCannotBindShift()
        {
            Assert.That(GameInput.DefaultKeyOf(Act.Dodge),Is.EqualTo(UnityEngine.InputSystem.Key.None));
            Assert.That(GameInput.RebindOrder,Has.No.Member(Act.Dodge));
            var before=GameInput.KeyOf(Act.Dodge);
            GameInput.Rebind(Act.Dodge,UnityEngine.InputSystem.Key.LeftShift);
            Assert.That(GameInput.KeyOf(Act.Dodge),Is.EqualTo(before));
            Assert.That(GameInput.Label(Act.Dodge),Is.EqualTo(GameInput.Label(Act.Left)+GameInput.Label(Act.Left)+" / "+GameInput.Label(Act.Right)+GameInput.Label(Act.Right)));
        }
    }
}

