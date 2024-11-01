using EPOOutline;
using DG.Tweening;
using UnityEngine;
using System.Collections;
// using static DG.Tweening.DOTweenModuleEPO;
// using static DG.Tweening.DOTweenModuleEPOOutline;

namespace _KMH_Framework
{
    public class OutlinableEx : Outlinable
    {
        [SerializeField]
        protected float _flickeringDuration;
        public float FlickeringDuration
        {
            get
            {
                return _flickeringDuration;
            }
            set
            {
                _flickeringDuration = value;
            }
        }

        [SerializeField]
        protected float _highlightDuration;
        public float HighlightDuration
        {
            get
            {
                return _highlightDuration;
            }
            set
            {
                _highlightDuration = value;
            }
        }

        protected Coroutine _flickeringRoutine = null;

        protected Color startFrontParamColor;
        protected Color endFrontParamColor;

        protected Color startFrontParamPublicColor;
        protected Color endFrontParamPublicColor;

        protected Color startBackParamColor;
        protected Color endBackParamColor;

        protected override void OnEnable()
        {
            UpdateVisibility();

            this.FrontParameters.DOKill(true);

            startFrontParamColor = new Color(this.FrontParameters.Color.r, this.FrontParameters.Color.g, this.FrontParameters.Color.b, 0f);
            endFrontParamColor = this.FrontParameters.Color;

            Color _publicColor = this.FrontParameters.FillPass.GetColor("_PublicColor");
            startFrontParamPublicColor = new Color(_publicColor.r, _publicColor.g, _publicColor.b, 0f);
            endFrontParamPublicColor = _publicColor;

            startBackParamColor = new Color(this.BackParameters.Color.r, this.BackParameters.Color.g, this.BackParameters.Color.b, 0f);
            endBackParamColor = this.BackParameters.Color;

            this.FrontParameters.Color = startFrontParamColor;
            this.FrontParameters.FillPass.SetColor("_PublicColor", startFrontParamPublicColor);
            this.BackParameters.Color = startBackParamColor;

            _flickeringRoutine = StartCoroutine(PostOnEnable_Flickering());
            StartCoroutine(PostOnEnable_CheckHighlightDuration());
        }

        protected IEnumerator PostOnEnable_Flickering()
        {
            float frontAlpha = endFrontParamColor.a;
            float frontPublicAlpha = endFrontParamPublicColor.a;
            float backAlpha = endBackParamColor.a;

            float halfOfDuration = FlickeringDuration / 2f;
            while (true)
            {
                this.FrontParameters.DOFade(frontAlpha, halfOfDuration);
                this.FrontParameters.FillPass.DOFade("_PublicColor", frontPublicAlpha, halfOfDuration);
                this.BackParameters.DOFade(backAlpha, halfOfDuration);

                yield return new WaitForSeconds(halfOfDuration);

                this.FrontParameters.DOFade(0f, halfOfDuration);
                this.FrontParameters.FillPass.DOFade("_PublicColor", 0f, halfOfDuration);
                this.BackParameters.DOFade(0f, halfOfDuration);

                yield return new WaitForSeconds(halfOfDuration);
            }
        }

        protected IEnumerator PostOnEnable_CheckHighlightDuration()
        {
            if (HighlightDuration == 0f)
            {
                yield break;
            }
            yield return new WaitForSeconds(HighlightDuration);

            this.enabled = false;
        }

        public IEnumerator ClickedFxRoutine(Color _frontColor, Color _frontPublicColor, Color _backColor, float _flickeringDuration)
        {
            if (_flickeringRoutine != null)
            {
                StopCoroutine(_flickeringRoutine);
            }

            float halfOfDuration = _flickeringDuration / 2f;

            this.FrontParameters.DOColor(_frontColor, halfOfDuration);
            this.FrontParameters.FillPass.DOColor("_PublicColor", _frontPublicColor, halfOfDuration);
            this.BackParameters.DOColor(_backColor, halfOfDuration);

            yield return new WaitForSeconds(halfOfDuration);

            this.FrontParameters.DOFade(0f, halfOfDuration);
            this.FrontParameters.FillPass.DOFade("_PublicColor", 0f, halfOfDuration);
            this.BackParameters.DOFade(0f, halfOfDuration);

            yield return new WaitForSeconds(halfOfDuration);

            this.enabled = false;
        }

        protected override void OnDisable()
        {
            if (_flickeringRoutine != null)
            {
                StopCoroutine(_flickeringRoutine);
            }

            outlinables.Remove(this);
        }

        protected void Reset()
        {
            SetSettingsPreset();
        }

        public void SetSettingsPreset()
        {
            this.enabled = false;
            this.FlickeringDuration = 0.5f;

            this.ComplexMaskingMode = ComplexMaskingMode.None;
            this.DrawingMode = OutlinableDrawingMode.Normal;
            this.OutlineLayer = 0;
            this.RenderStyle = RenderStyle.FrontBack;

            this.BackParameters.Enabled = true;
            this.BackParameters.Color = new Color(1f, 0f, 0f, 1f);
            this.BackParameters.DilateShift = 1f;
            this.BackParameters.BlurShift = 1f;

            this.FrontParameters.Enabled = true;
            this.FrontParameters.Color = new Color(1f, 0f, 0f, 1f);
            this.FrontParameters.DilateShift = 1f;
            this.FrontParameters.BlurShift = 1f;
            this.FrontParameters.FillPass.SetColor("_PublicColor", new Color(1f, 0f, 0f, 0.5f));

            Shader colorFillShader = Resources.Load("Easy performant outline/Shaders/Fills/ColorFill") as Shader;
            this.FrontParameters.FillPass.Shader = colorFillShader;

            this.AddAllChildRenderersToRenderingList(EPOOutline.RenderersAddingMode.MeshRenderer);
        }
    }
}