using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System;

[Serializable, VolumeComponentMenu("Post-processing/Custom/Outline")]
public sealed class AnisotropicKuwahara : CustomPostProcessVolumeComponent, IPostProcessComponent
{
    [Range(2, 20)]
    public ClampedIntParameter kernelSize = new ClampedIntParameter(2, 0, 10);
    [Range(1.0f, 18.0f)]
    public ClampedFloatParameter sharpness = new ClampedFloatParameter(8, 0f, 50f);
    [Range(1.0f, 100.0f)]
    public ClampedFloatParameter hardness = new ClampedFloatParameter(8, 0f, 50f);
    [Range(0.01f, 2.0f)]
    public ClampedFloatParameter alpha = new ClampedFloatParameter(1.0f, 0f, 10f);
    [Range(0.01f, 2.0f)]
    public ClampedFloatParameter zeroCrossing = new ClampedFloatParameter(0.58f, 0f, 10f);

    public BoolParameter useZeta = new BoolParameter(false);
    [Range(0.01f, 3.0f)]
    public ClampedFloatParameter zeta = new ClampedFloatParameter(1.0f, 0f, 10f);

    [Range(1, 4)]
    public ClampedIntParameter passes = new ClampedIntParameter(1, 0, 10);
    private Material kuwaharaMat;

    const string kShaderName = "Hidden/AnisotropicKuwahara";

    public override void Setup()
    {
        if (Shader.Find(kShaderName) != null)
        {
            kuwaharaMat = new Material(Shader.Find(kShaderName));
            kuwaharaMat.hideFlags = HideFlags.HideAndDontSave;
        }
        else
        {
            Debug.LogError($"Unable to find shader '{kShaderName}'. Post Process Volume NewPostProcessVolume is unable to load.");
        }
    }

    public override void Cleanup()
    {
        CoreUtils.Destroy(kuwaharaMat);
    }


    public bool IsActive() => kuwaharaMat != null;

    // Do not forget to add this post process in the Custom Post Process Orders list (Project Settings > Graphics > HDRP Global Settings).
    public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.AfterPostProcess;


    public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
    {
        if (kuwaharaMat == null)
            return;

        kuwaharaMat.SetInt("_KernelSize", kernelSize.value);
        kuwaharaMat.SetInt("_N", 8);
        kuwaharaMat.SetFloat("_Q", sharpness.value);
        kuwaharaMat.SetFloat("_Hardness", hardness.value);
        kuwaharaMat.SetFloat("_Alpha", alpha.value);
        kuwaharaMat.SetFloat("_ZeroCrossing", zeroCrossing.value);
        kuwaharaMat.SetFloat("_Zeta", useZeta.value ? zeta.value : 2.0f / 2.0f / (kernelSize.value / 2.0f));

        var structureTensor = RenderTexture.GetTemporary(source.rt.width, source.rt.height, 0, source.rt.format);
        Graphics.Blit(source, structureTensor, kuwaharaMat, 0);
        var eigenvectors1 = RenderTexture.GetTemporary(source.rt.width, source.rt.height, 0, source.rt.format);
        Graphics.Blit(structureTensor, eigenvectors1, kuwaharaMat, 1);
        var eigenvectors2 = RenderTexture.GetTemporary(source.rt.width, source.rt.height, 0, source.rt.format);
        Graphics.Blit(eigenvectors1, eigenvectors2, kuwaharaMat, 2);
        kuwaharaMat.SetTexture("_TFM", eigenvectors2);

        RenderTexture[] kuwaharaPasses = new RenderTexture[passes.value];

        for (int i = 0; i < passes.value; ++i)
        {
            kuwaharaPasses[i] = RenderTexture.GetTemporary(source.rt.width, source.rt.height, 0, source.rt.format);
        }

        Graphics.Blit(source, kuwaharaPasses[0], kuwaharaMat, 3);

        for (int i = 1; i < passes.value; ++i)
        {
            Graphics.Blit(kuwaharaPasses[i - 1], kuwaharaPasses[i], kuwaharaMat, 3);
        }

        //Graphics.Blit(structureTensor, destination);
        Graphics.Blit(kuwaharaPasses[passes.value - 1], destination);

        RenderTexture.ReleaseTemporary(structureTensor);
        RenderTexture.ReleaseTemporary(eigenvectors1);
        RenderTexture.ReleaseTemporary(eigenvectors2);
        for (int i = 0; i < passes.value; ++i)
        {
            RenderTexture.ReleaseTemporary(kuwaharaPasses[i]);
        }

        HDUtils.DrawFullScreen(cmd, kuwaharaMat, destination, shaderPassId: 0);
    }


    Texture2D _MainTex, _TFM;
    Vector4 _MainTex_TexelSize;
    int _KernelSize, _N, _Size;
    float _Hardness, _Q, _Alpha, _ZeroCrossing, _Zeta;

    float Gaussian(float sigma, float pos)
    {
        return (1.0f / Mathf.Sqrt(2.0f * Mathf.PI * sigma * sigma)) * Mathf.Exp(-(pos * pos) / (2.0f * sigma * sigma));
    }

    // Calculate Eigenvectors
    Vector4 Pass0()
    {
        Vector2 d = _MainTex_TexelSize.xy;

        Vector3 Sx = (
            1.0f * tex2D(_MainTex, i.uv + Vector2(-d.x, -d.y)).rgb +
            2.0f * tex2D(_MainTex, i.uv + Vector2(-d.x, 0.0)).rgb +
            1.0f * tex2D(_MainTex, i.uv + Vector2(-d.x, d.y)).rgb +
            -1.0f * tex2D(_MainTex, i.uv + Vector2(d.x, -d.y)).rgb +
            -2.0f * tex2D(_MainTex, i.uv + Vector2(d.x, 0.0)).rgb +
            -1.0f * tex2D(_MainTex, i.uv + Vector2(d.x, d.y)).rgb
        ) / 4.0f;

        Vector3 Sy = (
            1.0f * tex2D(_MainTex, i.uv + Vector2(-d.x, -d.y)).rgb +
            2.0f * tex2D(_MainTex, i.uv + Vector2(0.0, -d.y)).rgb +
            1.0f * tex2D(_MainTex, i.uv + Vector2(d.x, -d.y)).rgb +
            -1.0f * tex2D(_MainTex, i.uv + Vector2(-d.x, d.y)).rgb +
            -2.0f * tex2D(_MainTex, i.uv + Vector2(0.0, d.y)).rgb +
            -1.0f * tex2D(_MainTex, i.uv + Vector2(d.x, d.y)).rgb
        ) / 4.0f;


        return new Vector4(Vector3.Dot(Sx, Sx), Vector3.Dot(Sy, Sy), Vector3.Dot(Sx, Sy), 1.0f);
    }

    // Blur Pass 1
    Vector4 Pass1()
    {
        int kernelRadius = 5;

        Vector4 col = Vector4.zero;
        float kernelSum = 0.0f;

        for (int x = -kernelRadius; x <= kernelRadius; ++x)
        {
            Vector4 c = tex2D(_MainTex, i.uv + Vector2(x, 0) * _MainTex_TexelSize.xy);
            float gauss = Gaussian(2.0f, x);

            col += c * gauss;
            kernelSum += gauss;
        }

        return col / kernelSum;
    }

    // Blur Pass 2
    Vector4 Pass2()
    {
        int kernelRadius = 5;

        Vector4 col = Vector4.zero;
        float kernelSum = 0.0f;

        for (int y = -kernelRadius; y <= kernelRadius; ++y)
        {
            Vector4 c = tex2D(_MainTex, i.uv + new Vector2(0, y) * _MainTex_TexelSize.xy);
            float gauss = Gaussian(2.0f, y);

            col += c * gauss;
            kernelSum += gauss;
        }

        Vector3 g = col / kernelSum;

        float lambda1 = 0.5f * (g.y + g.x + Mathf.Sqrt(g.y * g.y - 2.0f * g.x * g.y + g.x * g.x + 4.0f * g.z * g.z));
        float lambda2 = 0.5f * (g.y + g.x - Mathf.Sqrt(g.y * g.y - 2.0f * g.x * g.y + g.x * g.x + 4.0f * g.z * g.z));

        Vector2 v = new Vector2(lambda1 - g.x, -g.z);
        Vector2 t = v.magnitude > 0.0 ? v.normalized : new Vector2(0.0f, 1.0f);
        float phi = -Mathf.Atan2(t.y, t.x);

        float A = (lambda1 + lambda2 > 0.0f) ? (lambda1 - lambda2) / (lambda1 + lambda2) : 0.0f;

        return new Vector4(t.x,t.y, phi, A);
    }

    // Apply Kuwahara Filter
    Vector4 Pass3()
    {
        float alpha = _Alpha;
        Vector4 t = tex2D(_TFM, i.uv);

        int kernelRadius = _KernelSize / 2;
        float a = kernelRadius * Mathf.Clamp((alpha + t.w) / alpha, 0.1f, 2.0f);
        float b = kernelRadius * Mathf.Clamp(alpha / (alpha + t.w), 0.1f, 2.0f);

        float cos_phi = Mathf.Cos(t.z);
        float sin_phi = Mathf.Sin(t.z);

        //THESE ARE ALL 2X2 MATRIXES

        Vector4 R = new Vector4(cos_phi, -sin_phi,
                              sin_phi, cos_phi);

        Vector4 S = new Vector4(0.5f / a, 0.0f,
                              0.0f, 0.5f / b);

        Vector4 SR = new Vector4(S * R);

        int max_x = (int)Mathf.Sqrt(a * a * cos_phi * cos_phi + b * b * sin_phi * sin_phi);
        int max_y = (int)Mathf.Sqrt(a * a * sin_phi * sin_phi + b * b * cos_phi * cos_phi);

        //float zeta = 2.0f / (kernelRadius);
        float zeta = _Zeta;

        float zeroCross = _ZeroCrossing;
        float sinZeroCross = Mathf.Sin(zeroCross);
        float eta = (zeta + Mathf.Cos(zeroCross)) / (sinZeroCross * sinZeroCross);
        int k;
        Vector4[] m = new Vector4[8];
        Vector3[] s = new Vector3[8];

        for (k = 0; k < _N; ++k)
        {
            m[k] = Vector4.zero;
            s[k] = Vector4.zero;
        }

        for (int y = -max_y; y <= max_y; ++y)
        {
            for (int x = -max_x; x <= max_x; ++x)
            {
                Vector2 v = mul(SR, Vector2(x, y));
                if (Vector2.Dot(v, v) <= 0.25f)
                {
                    Vector3 c = tex2D(_MainTex, i.uv + Vector2(x, y) * _MainTex_TexelSize.xy).rgb;
                    c = new Vector3(Mathf.Min(1, c.x), Mathf.Min(1, c.y), Mathf.Min(1, c.z));
                    float sum = 0;
                    float[] w = new float[8];
                    float z, vxx, vyy;

                    /* Calculate Polynomial Weights */
                    vxx = zeta - eta * v.x * v.x;
                    vyy = zeta - eta * v.y * v.y;
                    z = Mathf.Max(0, v.y + vxx);
                    w[0] = z * z;
                    sum += w[0];
                    z = Mathf.Max(0, -v.x + vyy);
                    w[2] = z * z;
                    sum += w[2];
                    z = Mathf.Max(0, -v.y + vxx);
                    w[4] = z * z;
                    sum += w[4];
                    z = Mathf.Max(0, v.x + vyy);
                    w[6] = z * z;
                    sum += w[6];
                    v = Mathf.Sqrt(2.0f) / 2.0f * new Vector2(v.x - v.y, v.x + v.y);
                    vxx = zeta - eta * v.x * v.x;
                    vyy = zeta - eta * v.y * v.y;
                    z = Mathf.Max(0, v.y + vxx);
                    w[1] = z * z;
                    sum += w[1];
                    z = Mathf.Max(0, -v.x + vyy);
                    w[3] = z * z;
                    sum += w[3];
                    z = Mathf.Max(0, -v.y + vxx);
                    w[5] = z * z;
                    sum += w[5];
                    z = Mathf.Max(0, v.x + vyy);
                    w[7] = z * z;
                    sum += w[7];

                    float g = Mathf.Exp(-3.125f * Vector2.Dot(v, v)) / sum;

                    for (int k = 0; k < 8; ++k)
                    {
                        float wk = w[k] * g;
                        m[k] += new Vector4(c * wk, wk);
                        s[k] += c * c * wk;
                    }
                }
            }
        }

        Vector4 output = Vector4.zero;
        for (k = 0; k < _N; ++k)
        {
            m[k].rgb /= m[k].w;
            s[k] = Mathf.Abs(s[k] / m[k].w - m[k].rgb * m[k].rgb);

            float sigma2 = s[k].x + s[k].y + s[k].z;
            float w = 1.0f / (1.0f + Mathf.Pow(_Hardness * 1000.0f * sigma2, 0.5f * _Q));

            output = new Vector4(m[k].x * w, m[k].y * w, m[k].z * w, w);
        }
        output = output / output.w;
        return new Vector4(Mathf.Min(1, output.x), Mathf.Min(1, output.y), Mathf.Min(1, output.z), Mathf.Min(1, output.w));
    }
}