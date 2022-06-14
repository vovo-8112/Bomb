// UI Editable properties
uniform sampler2D	_FaceTex;
uniform float		_FaceUVSpeedX;
uniform float		_FaceUVSpeedY;
uniform fixed4		_FaceColor;
uniform float		_FaceDilate;
uniform float		_OutlineSoftness;

uniform sampler2D	_OutlineTex;
uniform float		_OutlineUVSpeedX;
uniform float		_OutlineUVSpeedY;
uniform fixed4		_OutlineColor;
uniform float		_OutlineWidth;

uniform float		_Bevel;
uniform float		_BevelOffset;
uniform float		_BevelWidth;
uniform float		_BevelClamp;
uniform float		_BevelRoundness;

uniform sampler2D	_BumpMap;
uniform float		_BumpOutline;
uniform float		_BumpFace;

uniform samplerCUBE	_Cube;
uniform fixed4 		_ReflectFaceColor;
uniform fixed4		_ReflectOutlineColor;
uniform float3      _EnvMatrixRotation;
uniform float4x4	_EnvMatrix;

uniform fixed4		_SpecularColor;
uniform float		_LightAngle;
uniform float		_SpecularPower;
uniform float		_Reflectivity;
uniform float		_Diffuse;
uniform float		_Ambient;

uniform fixed4		_UnderlayColor;
uniform float		_UnderlayOffsetX;
uniform float		_UnderlayOffsetY;
uniform float		_UnderlayDilate;
uniform float		_UnderlaySoftness;

uniform fixed4 		_GlowColor;
uniform float 		_GlowOffset;
uniform float 		_GlowOuter;
uniform float 		_GlowInner;
uniform float 		_GlowPower;
uniform float 		_ShaderFlags;
uniform float		_WeightNormal;
uniform float		_WeightBold;

uniform float		_ScaleRatioA;
uniform float		_ScaleRatioB;
uniform float		_ScaleRatioC;

uniform float		_VertexOffsetX;
uniform float		_VertexOffsetY;
uniform float		_MaskID;
uniform sampler2D	_MaskTex;
uniform float4		_MaskCoord;
uniform float4		_ClipRect;

uniform float		_MaskSoftnessX;
uniform float		_MaskSoftnessY;
uniform sampler2D	_MainTex;
uniform float		_TextureWidth;
uniform float		_TextureHeight;
uniform float 		_GradientScale;
uniform float		_ScaleX;
uniform float		_ScaleY;
uniform float		_PerspectiveFilter;
uniform float		_Sharpness;
