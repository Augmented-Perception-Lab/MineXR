using System.Linq;
using System;
using UnityEngine;
public class Util : MonoBehaviour
{
    public static Matrix4x4 StringToMatrix4x4(string matrixstring) {
        Debug.Log($"RECONSTRUCT: converting {matrixstring}");
        //[[[0.98241621255874634,-0.027676457539200783,0.18464130163192749,0],[0.027725478634238243,0.99961286783218384,0.0023168351035565138,0],[-0.18463386595249176,0.0028431704267859459,0.98280328512191772,0],[-0.17760342359542847,0.10937044024467468,-0.78800058364868164,0.99999988079071045]]]
        // "[[ [m11, m12, m13, m14], [m21, m22, m23, m24], [m31, m32, m33, m34], [m41, m42, m43, m44] ]]"
        
        string[] tokens = matrixstring.Split(']');
        float[] m = new float[16];
        // tokens[0] = "[[[m11,m12,m13,m14"
        // tokens[1] = "[m21,m22,m23,m24"
        // tokens[2] = "[m31,m32,m33,m34"
        // tokens[3] = "[m41,m42,m43,m44]]]"

        char[] charsToTrim = {',', '[', ']'};
        for (var i = 0; i < 4; i++)
        {
            tokens[i] = tokens[i].Trim(charsToTrim);
            Debug.Log($"RECONSTRUCT: tokens[{i}]: {tokens[i]}");
            float[] row = Array.ConvertAll(tokens[i].Split(','), float.Parse);
            for (var j = 0; j < 4; j++)
            {
                m[i*4 + j] = row[j];
            }
        }
        var matrix = Matrix4x4.identity;
        matrix.m00 = m[0];
        matrix.m10 = m[1];
        matrix.m20 = m[2];
        matrix.m30 = m[3];
        matrix.m01 = m[4];
        matrix.m11 = m[5];
        matrix.m21 = m[6];
        matrix.m31 = m[7];
        matrix.m02 = m[8];
        matrix.m12 = m[9];
        matrix.m22 = m[10];
        matrix.m32 = m[11];
        matrix.m03 = m[12];
        matrix.m13 = m[13];
        matrix.m23 = m[14];
        matrix.m33 = m[15];


        return matrix;
        // return new Matrix4x4(m[0], m[1], m[2], m[3], m[4], m[5], m[6], m[7], m[8], m[9], m[10], m[11], m[12], m[13], m[14], m[15]);
        // return new Matrix4x4(m11, m12, m13, m14, m21, m22, m23, m24, m31, m32, m33, m34, m41, m42, m43, m44);
    }

    /// <summary>
    /// Extract translation from transform matrix.
    /// </summary>
    /// <param name="matrix">Transform matrix. This parameter is passed by reference
    /// to improve performance; no changes will be made to it.</param>
    /// <returns>
    /// Translation offset.
    /// </returns>
    public static Vector3 ExtractTranslationFromMatrix(ref Matrix4x4 matrix) {
        Vector3 translate;
        translate.x = -matrix.m03;
        translate.y = matrix.m13;
        translate.z = matrix.m23;
        return translate;
    }

    /// <summary>
    /// Extract rotation quaternion from transform matrix.
    /// </summary>
    /// <param name="matrix">Transform matrix. This parameter is passed by reference
    /// to improve performance; no changes will be made to it.</param>
    /// <returns>
    /// Quaternion representation of rotation transform.
    /// </returns>
    public static Quaternion ExtractRotationFromMatrix(ref Matrix4x4 matrix) {
        Vector3 forward;
        forward.x = matrix.m02;
        forward.y = -matrix.m12;
        forward.z = -matrix.m22;
    
        Vector3 upwards;
        upwards.x = matrix.m01;
        upwards.y = matrix.m11;
        upwards.z = matrix.m21;
    
        return Quaternion.LookRotation(forward, upwards);
    }
    
    /// <summary>
    /// Extract scale from transform matrix.
    /// </summary>
    /// <param name="matrix">Transform matrix. This parameter is passed by reference
    /// to improve performance; no changes will be made to it.</param>
    /// <returns>
    /// Scale vector.
    /// </returns>
    public static Vector3 ExtractScaleFromMatrix(ref Matrix4x4 matrix) {
        Vector3 scale;
        scale.x = new Vector4(matrix.m00, matrix.m10, matrix.m20, matrix.m30).magnitude;
        scale.y = new Vector4(matrix.m01, matrix.m11, matrix.m21, matrix.m31).magnitude;
        scale.z = new Vector4(matrix.m02, matrix.m12, matrix.m22, matrix.m32).magnitude;
        return scale;
    }
    
    /// <summary>
    /// Extract position, rotation and scale from TRS matrix.
    /// </summary>
    /// <param name="matrix">Transform matrix. This parameter is passed by reference
    /// to improve performance; no changes will be made to it.</param>
    /// <param name="localPosition">Output position.</param>
    /// <param name="localRotation">Output rotation.</param>
    /// <param name="localScale">Output scale.</param>
    public static void DecomposeMatrix(ref Matrix4x4 matrix, out Vector3 localPosition, out Quaternion localRotation, out Vector3 localScale) {
        localPosition = ExtractTranslationFromMatrix(ref matrix);
        localRotation = ExtractRotationFromMatrix(ref matrix);
        localScale = ExtractScaleFromMatrix(ref matrix);
    }
    
    /// <summary>
    /// Set transform component from TRS matrix.
    /// </summary>
    /// <param name="transform">Transform component.</param>
    /// <param name="matrix">Transform matrix. This parameter is passed by reference
    /// to improve performance; no changes will be made to it.</param>
    public static void SetTransformFromMatrix(Transform transform, ref Matrix4x4 matrix) {
        transform.localPosition = ExtractTranslationFromMatrix(ref matrix);
        transform.localRotation = ExtractRotationFromMatrix(ref matrix);
        transform.localScale = ExtractScaleFromMatrix(ref matrix);
    }
    
    
    // EXTRAS!
    
    /// <summary>
    /// Identity quaternion.
    /// </summary>
    /// <remarks>
    /// <para>It is faster to access this variation than <c>Quaternion.identity</c>.</para>
    /// </remarks>
    public static readonly Quaternion IdentityQuaternion = Quaternion.identity;
    /// <summary>
    /// Identity matrix.
    /// </summary>
    /// <remarks>
    /// <para>It is faster to access this variation than <c>Matrix4x4.identity</c>.</para>
    /// </remarks>
    public static readonly Matrix4x4 IdentityMatrix = Matrix4x4.identity;
    
    /// <summary>
    /// Get translation matrix.
    /// </summary>
    /// <param name="offset">Translation offset.</param>
    /// <returns>
    /// The translation transform matrix.
    /// </returns>
    public static Matrix4x4 TranslationMatrix(Vector3 offset) {
        Matrix4x4 matrix = IdentityMatrix;
        matrix.m03 = offset.x;
        matrix.m13 = offset.y;
        matrix.m23 = offset.z;
        return matrix;
    }
}   