using System;
using UIForia.Layout;
using UIForia.Rendering.Vertigo;
using UIForia.Util;
using UnityEngine;
using UnityEngine.UI;

namespace UIForia.Rendering {

    public class UIForiaGeometry {

        public StructList<Vector3> positionList;
        public StructList<Vector4> texCoordList0;
        public StructList<Vector4> texCoordList1;
        public StructList<int> triangleList;

        public Vector4 packedColors;
        public Vector4 objectData;
        public Texture mainTexture;
        public Vector4 miscData;
        public Vector4 cornerData;

        public UIForiaGeometry() {
            this.positionList = new StructList<Vector3>();
            this.texCoordList0 = new StructList<Vector4>();
            this.texCoordList1 = new StructList<Vector4>();
            this.triangleList = new StructList<int>();
        }

        public void EnsureAdditionalCapacity(int vertexCount, int triangleCount) {
            positionList.EnsureAdditionalCapacity(vertexCount);
            texCoordList0.EnsureAdditionalCapacity(vertexCount);
            texCoordList1.EnsureAdditionalCapacity(vertexCount);
            triangleList.EnsureAdditionalCapacity(triangleCount);
        }

        public void Clear() {
            mainTexture = null;
            objectData = default;
            packedColors = default;
            positionList.size = 0;
            texCoordList0.size = 0;
            texCoordList1.size = 0;
            triangleList.size = 0;
        }

        public void UpdateSizes(int vertexCount, int triangleCount) {
            positionList.size += vertexCount;
            texCoordList0.size += vertexCount;
            texCoordList1.size += vertexCount;
            triangleList.size += triangleCount;
        }

        public void Quad(float width, float height) {
            EnsureAdditionalCapacity(4, 6);

            Vector3[] positions = positionList.array;
            Vector4[] texCoord0 = texCoordList0.array;
            int[] triangles = triangleList.array;

            int startVert = positionList.size;
            int startTriangle = triangleList.size;

            ref Vector3 p0 = ref positions[startVert + 0];
            ref Vector3 p1 = ref positions[startVert + 1];
            ref Vector3 p2 = ref positions[startVert + 2];
            ref Vector3 p3 = ref positions[startVert + 3];

            ref Vector4 uv0 = ref texCoord0[startVert + 0];
            ref Vector4 uv1 = ref texCoord0[startVert + 1];
            ref Vector4 uv2 = ref texCoord0[startVert + 2];
            ref Vector4 uv3 = ref texCoord0[startVert + 3];

            p0.x = 0;
            p0.y = 0;
            p0.z = 0;

            p1.x = width;
            p1.y = 0;
            p1.z = 0;

            p2.x = width;
            p2.y = -height;
            p2.z = 0;

            p3.x = 0;
            p3.y = -height;
            p3.z = 0;

            uv0.x = 0;
            uv0.y = 1;

            uv1.x = 1;
            uv1.y = 1;

            uv2.x = 1;
            uv2.y = 0;

            uv3.x = 0;
            uv3.y = 0;

            triangles[startTriangle + 0] = startVert + 0;
            triangles[startTriangle + 1] = startVert + 1;
            triangles[startTriangle + 2] = startVert + 2;
            triangles[startTriangle + 3] = startVert + 2;
            triangles[startTriangle + 4] = startVert + 3;
            triangles[startTriangle + 5] = startVert + 0;

            UpdateSizes(4, 6);
        }

        public void ClipCornerRect(Size size, in CornerDefinition cornerDefinition, in Vector2 position = default) {
            EnsureAdditionalCapacity(9, 24);
            Vector3[] positions = positionList.array;
            Vector4[] texCoord0 = texCoordList0.array;
            int[] triangles = triangleList.array;

            int startVert = positionList.size;
            int startTriangle = triangleList.size;

            float width = size.width;
            float height = size.height;

            positions[startVert + 0] = new Vector3(position.x + 0, -(position.y + cornerDefinition.topLeftY), 0);
            positions[startVert + 1] = new Vector3(position.x + cornerDefinition.topLeftX, -position.y, 0);
            positions[startVert + 2] = new Vector3(position.x + width - cornerDefinition.topRightX, -position.y, 0);
            positions[startVert + 3] = new Vector3(position.x + width, -(position.y + cornerDefinition.topRightY), 0);
            positions[startVert + 4] = new Vector3(position.x + width, -(position.y + height - cornerDefinition.bottomRightY), 0);
            positions[startVert + 5] = new Vector3(position.x + width - cornerDefinition.bottomRightX, -(position.y + height), 0);
            positions[startVert + 6] = new Vector3(position.x + cornerDefinition.bottomLeftX, -(position.y + height), 0);
            positions[startVert + 7] = new Vector3(position.x + 0, -(position.y + height - cornerDefinition.bottomLeftY), 0);
            positions[startVert + 8] = new Vector3(position.x + width * 0.5f, -(position.y + (height * 0.5f)), 0);

            triangles[startTriangle + 0] = startVert + 1;
            triangles[startTriangle + 1] = startVert + 8;
            triangles[startTriangle + 2] = startVert + 0;

            triangles[startTriangle + 3] = startVert + 2;
            triangles[startTriangle + 4] = startVert + 8;
            triangles[startTriangle + 5] = startVert + 1;

            triangles[startTriangle + 6] = startVert + 3;
            triangles[startTriangle + 7] = startVert + 8;
            triangles[startTriangle + 8] = startVert + 2;

            triangles[startTriangle + 9] = startVert + 4;
            triangles[startTriangle + 10] = startVert + 8;
            triangles[startTriangle + 11] = startVert + 3;

            triangles[startTriangle + 12] = startVert + 5;
            triangles[startTriangle + 13] = startVert + 8;
            triangles[startTriangle + 14] = startVert + 4;

            triangles[startTriangle + 15] = startVert + 6;
            triangles[startTriangle + 16] = startVert + 8;
            triangles[startTriangle + 17] = startVert + 5;

            triangles[startTriangle + 18] = startVert + 7;
            triangles[startTriangle + 19] = startVert + 8;
            triangles[startTriangle + 20] = startVert + 6;

            triangles[startTriangle + 21] = startVert + 0;
            triangles[startTriangle + 22] = startVert + 8;
            triangles[startTriangle + 23] = startVert + 7;

            for (int i = 0; i < 9; i++) {
                float x = (positions[startVert + i].x - position.x) / width;
                float y = 1 - ((positions[startVert + i].y + position.y) / -height);
                texCoord0[startVert + i] = new Vector4(x, y, x, y);
            }

            triangleList.size += 24;
            positionList.size += 9;
            texCoordList0.size += 9;
            texCoordList1.size += 9;
        }

        public void FillRect(float width, float height, in Vector2 position = default) {
            Vector3[] positions = positionList.array;
            Vector4[] texCoord0 = texCoordList0.array;
            int[] triangles = triangleList.array;

            int startVert = positionList.size;
            int startTriangle = triangleList.size;

            ref Vector3 p0 = ref positions[startVert + 0];
            ref Vector3 p1 = ref positions[startVert + 1];
            ref Vector3 p2 = ref positions[startVert + 2];
            ref Vector3 p3 = ref positions[startVert + 3];

            ref Vector4 uv0 = ref texCoord0[startVert + 0];
            ref Vector4 uv1 = ref texCoord0[startVert + 1];
            ref Vector4 uv2 = ref texCoord0[startVert + 2];
            ref Vector4 uv3 = ref texCoord0[startVert + 3];

            p0.x = (position.x + 0);
            p0.y = -position.y;
            p0.z = 0;

            p1.x = position.x + width;
            p1.y = -position.y;
            p1.z = 0;

            p2.x = position.x + width;
            p2.y = -(position.y + height);
            p2.z = 0;

            p3.x = position.x;
            p3.y = -(position.y + height);
            p3.z = 0;

//            p0 -= new Vector3(100, -100, 0);
//            p1 -= new Vector3(100, -100, 0);
//            p2 -= new Vector3(100, -100, 0);
//            p3 -= new Vector3(100, -100, 0);

            uv0.x = 0;
            uv0.y = 1;
            uv0.z = 0;
            uv0.w = 1;

            uv1.x = 1;
            uv1.y = 1;
            uv1.z = 1;
            uv1.w = 1;

            uv2.x = 1;
            uv2.y = 0;
            uv2.z = 1;
            uv2.w = 0;

            uv3.x = 0;
            uv3.y = 0;
            uv3.z = 0;
            uv3.w = 0;

            triangles[startTriangle + 0] = startVert + 0;
            triangles[startTriangle + 1] = startVert + 1;
            triangles[startTriangle + 2] = startVert + 2;
            triangles[startTriangle + 3] = startVert + 2;
            triangles[startTriangle + 4] = startVert + 3;
            triangles[startTriangle + 5] = startVert + 0;

            positionList.size += 4;
            texCoordList0.size += 4;
            texCoordList1.size += 4;
            triangleList.size += 6;
        }

        private static readonly Vector3[] s_Xy = new Vector3[4];
        private static readonly Vector3[] s_Uv = new Vector3[4];

        public void FillMeshType(Rect rect, MeshType fillMethod, MeshFillOrigin fillOrigin, float fillAmount, MeshFillDirection fillClockwise) {
            GenerateFilledSprite(new Vector4(0, -rect.height, rect.width, 0), fillMethod, (int) fillOrigin, fillAmount, fillClockwise == MeshFillDirection.Clockwise, Color.white);
        }

        private void GenerateFilledSprite(Vector4 v, MeshType fillMethod, int m_FillOrigin, float m_FillAmount, bool m_FillClockwise, Color color) {

            positionList.size = 0;
            triangleList.size = 0;
            texCoordList0.size = 0;
            texCoordList1.size = 0;
           FillRect(200, 200);

            if (m_FillAmount < 0.001f) {
                return;
            }
            
            float tx0 = 0;//outer.x;
            float ty0 = 1; //outer.y;
            float tx1 = 1; //outer.z;
            float ty1 = 0; //outer.w;
            s_Uv[0] = new Vector2(tx0, ty0);
            s_Uv[2] = new Vector2(tx1, ty0);
            s_Uv[1] = new Vector2(tx1, ty1);
            s_Uv[3] = new Vector2(tx0, ty1);
            s_Xy[0] = new Vector2(v.x, v.y);
            s_Xy[1] = new Vector2(v.x, v.w);
            s_Xy[2] = new Vector2(v.z, v.w);
            s_Xy[3] = new Vector2(v.z, v.y);
            positionList.size = 0;
            triangleList.size = 0;
            texCoordList0.size = 0;
            texCoordList1.size = 0;
            // Horizontal and vertical filled sprites are simple -- just end the Image prematurely
            if (fillMethod == MeshType.FillHorizontal || fillMethod == MeshType.FillVertical) {
                if (fillMethod == MeshType.FillHorizontal) {
                    float fill = (tx1 - tx0) * m_FillAmount;

                    if (m_FillOrigin == 1) {
                        v.x = v.z - (v.z - v.x) * m_FillAmount;
                        tx0 = tx1 - fill;
                    }
                    else {
                        v.z = v.x + (v.z - v.x) * m_FillAmount;
                        tx1 = tx0 + fill;
                    }
                }
                else if (fillMethod == MeshType.FillVertical) {
                    float fill = (ty1 - ty0) * m_FillAmount;

                    if (m_FillOrigin == 1) {
                        v.y = v.w - (v.w - v.y) * m_FillAmount;
                        ty0 = ty1 - fill;
                    }
                    else {
                        v.w = v.y + (v.w - v.y) * m_FillAmount;
                        ty1 = ty0 + fill;
                    }
                }
            }

            s_Xy[0] = new Vector2(v.x, v.y);
            s_Xy[1] = new Vector2(v.x, v.w);
            s_Xy[2] = new Vector2(v.z, v.w);
            s_Xy[3] = new Vector2(v.z, v.y);

            s_Uv[0] = new Vector2(tx0, ty0);
            s_Uv[1] = new Vector2(tx1, ty0);
            s_Uv[2] = new Vector2(tx1, ty1);
            s_Uv[3] = new Vector2(tx0, ty1);

            {
                if (m_FillAmount < 1f && fillMethod != MeshType.FillHorizontal && fillMethod != MeshType.FillVertical) {
                    if (fillMethod == MeshType.FillRadial90) {
                        if (RadialCut(s_Xy, s_Uv, m_FillAmount, m_FillClockwise, m_FillOrigin))
                            AddQuad(s_Xy, s_Uv);
                    }
                    else if (fillMethod == MeshType.FillRadial180) {
                        for (int side = 0; side < 2; ++side) {
                            float fx0, fx1, fy0, fy1;
                            int even = m_FillOrigin > 1 ? 1 : 0;

                            if (m_FillOrigin == 0 || m_FillOrigin == 2) {
                                fy0 = 0f;
                                fy1 = 1f;
                                if (side == even) {
                                    fx0 = 0f;
                                    fx1 = 0.5f;
                                }
                                else {
                                    fx0 = 0.5f;
                                    fx1 = 1f;
                                }
                            }
                            else {
                                fx0 = 0f;
                                fx1 = 1f;
                                if (side == even) {
                                    fy0 = 0.5f;
                                    fy1 = 1f;
                                }
                                else {
                                    fy0 = 0f;
                                    fy1 = 0.5f;
                                }
                            }

                            s_Xy[0].x = Mathf.Lerp(v.x, v.z, fx0);
                            s_Xy[1].x = s_Xy[0].x;
                            s_Xy[2].x = Mathf.Lerp(v.x, v.z, fx1);
                            s_Xy[3].x = s_Xy[2].x;

                            s_Xy[0].y = Mathf.Lerp(v.y, v.w, fy0);
                            s_Xy[1].y = Mathf.Lerp(v.y, v.w, fy1);
                            s_Xy[2].y = s_Xy[1].y;
                            s_Xy[3].y = s_Xy[0].y;

                            s_Uv[0].x = Mathf.Lerp(tx0, tx1, fx0);
                            s_Uv[1].x = s_Uv[0].x;
                            s_Uv[2].x = Mathf.Lerp(tx0, tx1, fx1);
                            s_Uv[3].x = s_Uv[2].x;

                            s_Uv[0].y = Mathf.Lerp(ty0, ty1, fy0);
                            s_Uv[1].y = Mathf.Lerp(ty0, ty1, fy1);
                            s_Uv[2].y = s_Uv[1].y;
                            s_Uv[3].y = s_Uv[0].y;

                            float val = m_FillClockwise ? m_FillAmount * 2f - side : m_FillAmount * 2f - (1 - side);

                            if (RadialCut(s_Xy, s_Uv, Mathf.Clamp01(val), m_FillClockwise, ((side + m_FillOrigin + 3) % 4))) {
                                AddQuad(s_Xy, s_Uv);
                            }
                        }
                    }
                    else if (fillMethod == MeshType.FillRadial360) {
                        for (int corner = 0; corner < 4; ++corner) {
                            float fx0, fx1, fy0, fy1;

                            if (corner < 2) {
                                fx0 = 0f;
                                fx1 = 0.5f;
                            }
                            else {
                                fx0 = 0.5f;
                                fx1 = 1f;
                            }

                            if (corner == 0 || corner == 3) {
                                fy0 = 0f;
                                fy1 = 0.5f;
                            }
                            else {
                                fy0 = 0.5f;
                                fy1 = 1f;
                            }

                            s_Xy[0].x = Mathf.Lerp(v.x, v.z, fx0);
                            s_Xy[1].x = s_Xy[0].x;
                            s_Xy[2].x = Mathf.Lerp(v.x, v.z, fx1);
                            s_Xy[3].x = s_Xy[2].x;

                            s_Xy[0].y = Mathf.Lerp(v.y, v.w, fy0);
                            s_Xy[1].y = Mathf.Lerp(v.y, v.w, fy1);
                            s_Xy[2].y = s_Xy[1].y;
                            s_Xy[3].y = s_Xy[0].y;

                            s_Uv[0].x = Mathf.Lerp(tx0, tx1, fx0);
                            s_Uv[1].x = s_Uv[0].x;
                            s_Uv[2].x = Mathf.Lerp(tx0, tx1, fx1);
                            s_Uv[3].x = s_Uv[2].x;

                            s_Uv[0].y = Mathf.Lerp(ty0, ty1, fy0);
                            s_Uv[1].y = Mathf.Lerp(ty0, ty1, fy1);
                            s_Uv[2].y = s_Uv[1].y;
                            s_Uv[3].y = s_Uv[0].y;

                            float val = m_FillClockwise
                                ? m_FillAmount * 4f - ((corner + m_FillOrigin) % 4)
                                : m_FillAmount * 4f - (3 - ((corner + m_FillOrigin) % 4));

                            if (RadialCut(s_Xy, s_Uv, Mathf.Clamp01(val), m_FillClockwise, ((corner + 2) % 4)))
                                AddQuad(s_Xy, s_Uv);
                        }
                    }
                }
                else {
                    AddQuad(s_Xy, s_Uv);
                }
            }
        }

        private void AddQuad(Vector3[] quadPositions, Vector3[] quadUVs) {
            int startIndex = positionList.size;

            for (int i = 0; i < 4; ++i) {
                positionList.Add(quadPositions[i]);
                texCoordList0.Add(new Vector4(quadUVs[i].x, quadUVs[i].y, 0, 0));
                texCoordList1.Add(new Vector4(0, 0, 0, 0));
            }

            triangleList.Add(startIndex + 0);
            triangleList.Add(startIndex + 1);
            triangleList.Add(startIndex + 2);

            triangleList.Add(startIndex + 2);
            triangleList.Add(startIndex + 3);
            triangleList.Add(startIndex + 0);

            // vertexHelper.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
            // vertexHelper.AddTriangle(startIndex + 2, startIndex + 3, startIndex);
        }

        /// <summary>
        /// Adjust the specified quad, making it be radially filled instead.
        /// </summary>
        private static bool RadialCut(Vector3[] xy, Vector3[] uv, float fill, bool invert, int corner) {
            // Nothing to fill
            if (fill < 0.001f) return false;

            // Even corners invert the fill direction
            if ((corner & 1) == 1) invert = !invert;

            // Nothing to adjust
            if (!invert && fill > 0.999f) return true;

            // Convert 0-1 value into 0 to 90 degrees angle in radians
            float angle = Mathf.Clamp01(fill);
            if (invert) angle = 1f - angle;
            angle *= 90f * Mathf.Deg2Rad;

            // Calculate the effective X and Y factors
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);

            RadialCut(xy, cos, sin, invert, corner);
            RadialCut(uv, cos, sin, invert, corner);
            return true;
        }

        /// <summary>
        /// Adjust the specified quad, making it be radially filled instead.
        /// </summary>
        static void RadialCut(Vector3[] xy, float cos, float sin, bool invert, int corner) {
            int i0 = corner;
            int i1 = ((corner + 1) % 4);
            int i2 = ((corner + 2) % 4);
            int i3 = ((corner + 3) % 4);

            if ((corner & 1) == 1) {
                if (sin > cos) {
                    cos /= sin;
                    sin = 1f;

                    if (invert) {
                        xy[i1].x = Mathf.Lerp(xy[i0].x, xy[i2].x, cos);
                        xy[i2].x = xy[i1].x;
                    }
                }
                else if (cos > sin) {
                    sin /= cos;
                    cos = 1f;

                    if (!invert) {
                        xy[i2].y = Mathf.Lerp(xy[i0].y, xy[i2].y, sin);
                        xy[i3].y = xy[i2].y;
                    }
                }
                else {
                    cos = 1f;
                    sin = 1f;
                }

                if (!invert) xy[i3].x = Mathf.Lerp(xy[i0].x, xy[i2].x, cos);
                else xy[i1].y = Mathf.Lerp(xy[i0].y, xy[i2].y, sin);
            }
            else {
                if (cos > sin) {
                    sin /= cos;
                    cos = 1f;

                    if (!invert) {
                        xy[i1].y = Mathf.Lerp(xy[i0].y, xy[i2].y, sin);
                        xy[i2].y = xy[i1].y;
                    }
                }
                else if (sin > cos) {
                    cos /= sin;
                    sin = 1f;

                    if (invert) {
                        xy[i2].x = Mathf.Lerp(xy[i0].x, xy[i2].x, cos);
                        xy[i3].x = xy[i2].x;
                    }
                }
                else {
                    cos = 1f;
                    sin = 1f;
                }

                if (invert) xy[i3].y = Mathf.Lerp(xy[i0].y, xy[i2].y, sin);
                else xy[i1].x = Mathf.Lerp(xy[i0].x, xy[i2].x, cos);
            }
        }

        public void EnsureCapacity(int vertexCount, int triangleCount) {
            if (positionList.array.Length < vertexCount) {
                Array.Resize(ref positionList.array, vertexCount);
                Array.Resize(ref texCoordList0.array, vertexCount);
                Array.Resize(ref texCoordList1.array, vertexCount);
            }

            if (triangleList.array.Length < triangleCount) {
                Array.Resize(ref triangleList.array, triangleCount);
            }
        }

        public void ToMesh(PooledMesh mesh) {
            mesh.mesh.Clear();
            mesh.SetVertices(positionList.array, positionList.size);
            mesh.SetTextureCoord0(texCoordList0.array, texCoordList0.size);
            mesh.SetTextureCoord1(texCoordList1.array, texCoordList1.size);
            mesh.SetTriangles(triangleList.array, triangleList.size);
        }

    }

}