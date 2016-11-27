/*{
 "DESCRIPTION": "Omnidome UVW Map Shader",
 "CREDIT": "Michael Winkelmann",
 "CATEGORIES":
 [
 "Omnidome"
 ],
 "INPUTS":
 [
 {
 "NAME": "image",
 "LABEL": "Image",
 "TYPE": "image"
 },
 {
 "NAME": "uvw_map",
 "LABEL": "UVW Map",
 "TYPE": "image"
 },
 {
 "NAME": "map_mode",
 "VALUES": [
 0,
 1,
 2
 ],
 "LABELS": [
 "Equirectangular",
 "Fisheye",
 "Cubic"
 ],
 "DEFAULT": 0,
 "TYPE": "long"
 },
 {
 "NAME": "map_yaw",
 "LABEL": "Yaw",
 "TYPE": "float",
 "DEFAULT": 0.0,
 "MIN": -180.0,
 "MAX": 180.0
 },
 {
 "NAME": "map_pitch",
 "LABEL": "Pitch",
 "TYPE": "float",
 "DEFAULT": 0.0,
 "MIN": -180.0,
 "MAX": 180.0
 },
 {
 "NAME": "map_roll",
 "LABEL" : "Roll",
 "TYPE": "float",
 "DEFAULT": 0.0,
 "MIN": -180.0,
 "MAX": 180.0
 },
 {
 "NAME": "map_mirror_horizontal",
 "LABEL": "Mirror horizontal",
 "TYPE": "bool",
 "DEFAULT": 0
 },
 {
 "NAME": "map_mirror_vertical",
 "LABEL": "Mirror vertical",
 "TYPE": "bool",
 "DEFAULT": 0
 },
 {
 "NAME": "enable_color_calibration",
 "LABEL": "Color Calibration",
 "TYPE": "bool",
 "DEFAULT": 1
 }
 ]
 }
 */
/******************************************************************
 This file is part of Omnidome.

 DomeSimulator is free software: you can redistribute it and/or modify
 it under the terms of the GNU General Public License as published by
 the Free Software Foundation, either version 3 of the License, or
 (at your option) any later version.

 DomeSimulator is distributed in the hope that it will be useful,
 but WITHOUT ANY WARRANTY; without even the implied warranty of
 MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 GNU General Public License for more details.

 You should have received a copy of the GNU General Public License
 along with DomeSimulator.  If not, see <http://www.gnu.org/licenses/>.

 Omnidome is free for non-commercial use. If you want to use it
 commercially, you should contact the author
 Michael Winkelmann aka Wilston Oreo by mail:
 me@wilstonoreo.net
 **************************************************************************/

#define MAP_EQUIRECTANGULAR 0
#define MAP_FISHEYE 1
#define MAP_CUBE 2

vec3 uvw;

const float PI = 3.14159265358979323846264;

////////////////////////////////////////////
// Color correction
////////////////////////////////////////////

struct ChannelCorrection {
    float gamma;
    float brightness;
    float contrast;
    float multiplier;
};

ChannelCorrection cc_red, cc_green, cc_blue, cc_all;

float brightness(float v, float brightness_value) {
    return max(v + brightness_value,0.0);
}

/// Calculate contrast corrected value
float contrast(float v, float contrast_value) {
    float _c = (contrast_value <= 0.0) ?
        contrast_value + 1.0 :
        (1.0 / (1.0 - contrast_value));
    return (v - 0.5) * max(_c, 0.0) + 0.5;
}

/// Calculate gamma corrected value
float gamma(float v, float gamma_value) {
    float g = 0.0;
    if (gamma_value >= 0.0) {
        g = 1.0 / (1.0 + gamma_value);
    } else {
        g = (1.0 - gamma_value);
    }
    return pow(v,g*g*g*g);
}

float corrected(in ChannelCorrection c, in float v) {
    return brightness(contrast(gamma(v,
        c.gamma * c.multiplier),
        c.contrast * c.multiplier),
        c.brightness * c.multiplier);
}

#define ALL_CHANNEL_SHIFT 3.0
#define RED_CHANNEL_SHIFT 2.0
#define GREEN_CHANNEL_SHIFT 1.0
#define BLUE_CHANNEL_SHIFT 0.0




void getChannelCorrectionFromImage(
    in sampler2DRect image,
    in vec2 s, // Size
    in vec2 uv, // Pixel pos
    in float _channelShift,
    out ChannelCorrection c) {
        
	vec2 multiplier_pos = vec2(uv.x*s.x,s.y / 3.0 * (0.5 + 4.0*_channelShift) / 16.0); 
	vec2 contrast_pos = vec2(uv.x*s.x,s.y / 3.0 * (1.5 + 4.0*_channelShift) / 16.0); 
	vec2 brightness_pos = vec2(uv.x*s.x,s.y / 3.0 * (2.5 + 4.0*_channelShift) / 16.0); 
	vec2 gamma_pos = vec2(uv.x*s.x,s.y / 3.0 * (3.5 + 4.0*_channelShift) / 16.0);   
    
    c.gamma = (texture2DRect(image,gamma_pos).g - 0.5)*2.0;
    c.brightness = (texture2DRect(image,brightness_pos).g - 0.5)*2.0;
    c.contrast = (texture2DRect(image,contrast_pos).g - 0.5)*2.0;
    c.multiplier = (texture2DRect(image,multiplier_pos).g - 0.5)*2.0;
}

vec3 colorCorrection(vec3 color) {
	if (!enable_color_calibration) return color; 
    if (cc_all.multiplier == 0.0) {
        return color;
    }
    return clamp(vec3(
        corrected(cc_all,corrected(cc_red,color.r)),
        corrected(cc_all,corrected(cc_green,color.g)),
        corrected(cc_all,corrected(cc_blue,color.b))
    ),vec3(0.0,0.0,0.0),vec3(1.0,1.0,1.0));
}

////////////////////////////////////////////
// END Color correction
////////////////////////////////////////////


/// Convert degrees to radians
float deg2rad(in float deg)
{
    return deg * PI / 180.0;
}

/// Calculates the rotation matrix of a rotation around Z axis with an angle in radians
mat3 rotateAroundZ( in float angle )
{
    float s = sin(angle);
    float c = cos(angle);
    return mat3(  c, -s,0.0,
                s,  c,0.0,
                0.0,0.0,1.0);
}

/// Calculates the rotation matrix of a rotation around X axis with an angle in radians
mat3 rotateAroundX( in float angle )
{
    float s = sin(angle);
    float c = cos(angle);
    return mat3(1.0,0.0,0.0,
                0.0,  c, -s,
                0.0,  s,  c);
}

/// Calculates the rotation matrix of a rotation around Y axis with an angle in radians
mat3 rotateAroundY( in float angle )
{
    float s = sin(angle);
    float c = cos(angle);
    return mat3(  c,0.0,  s,
                0.0,1.0,0.0,
                -s,0.0,  c);
}

/// Calculate rotation by given yaw and pitch angles (in degrees!)
mat3 rotationMatrix(in float yaw, in float pitch, in float roll)
{
    return rotateAroundZ(deg2rad(yaw)) *
    rotateAroundY(deg2rad(-pitch)) *
    rotateAroundX(deg2rad(roll));
}


vec3 map_uvw()
{
    return rotationMatrix(map_yaw,map_pitch,map_roll) * uvw;
}

float map_equirectangular(out vec2 texCoords)
{
    vec3 uvw = map_uvw();
    texCoords.s = fract(atan(uvw.y,uvw.x) / (2.0*PI));
    texCoords.t = fract(acos(uvw.z) / PI);
    return 1.0;
}

float map_fisheye(out vec2 texCoords)
{
    vec3 n = map_uvw();
    float phi = atan(sqrt(n.x*n.x + n.y*n.y),n.z);
    float r =  phi / PI * 2.0;
    if ((r > 1.0) || (r <= 0.0)) return -1.0;
    float theta = atan(n.x,n.y);
    texCoords.s = fract(0.5 * (1.0 + r* cos(theta)));
    texCoords.t = fract(0.5 * (1.0 + r * sin(theta)));
    return 1.0;
}

float map_cube(out vec2 texCoords)
{
    vec3 n = map_uvw();
    float sc, tc, ma;
    float eps =  -0.015;
    float _off = 0.0;
    //n = n.yzx;

    if ((abs(n.x) >= abs(n.y)) && (abs(n.x) >= abs(n.z)))
    {
        sc = (n.x > 0.0) ? -n.y : n.y;
        tc = n.z;
        ma = n.x;
        _off += (n.x < 0.0) ? 0.0/6.0 : 2.0/6.0; // EAST / WEST
    } else
        if ((abs(n.y) >= abs(n.z)))
        {
            sc = (n.y < 0.0) ? -n.x : n.x;
            tc = n.z;
            ma = n.y;
            _off += (n.y < 0.0) ? 1.0/6.0 : 3.0/6.0; // NORTH / SOUTH
        } else
        {
            sc = (n.z > 0.0) ? n.y : n.y;
            tc = (n.z > 0.0) ? n.x : -n.x;
            ma = n.z;
            _off = (n.z < 0.0) ? 4.0/6.0 : 5.0 / 6.0;  // TOP / BOTTOM
        }
    texCoords = vec2(sc/(12.0 - eps)/abs(ma) + 0.5/6.0 + _off,0.5+tc/(2.0 - eps)/abs(ma)) ;
    return 1.0;
}

float mapping(out vec2 texCoords)
{
#ifdef MAP_EQUIRECTANGULAR
    if (map_mode == MAP_EQUIRECTANGULAR)
    {
        return map_equirectangular(texCoords);
    }
#endif
#ifdef MAP_FISHEYE
    if (map_mode == MAP_FISHEYE)
    {
        return map_fisheye(texCoords);
    }
#endif
#ifdef MAP_CUBE
    if (map_mode == MAP_CUBE)
    {
        return map_cube(texCoords);
    }
#endif
    return -1.0;
}


void main()
{
    gl_FragColor = vec4(0.0,0.0,0.0,1.0);

    vec2 s = _uvw_map_imgSize;
    
    vec2 uv = vv_FragNormCoord + vec2(0.0,1.0/_uvw_map_imgSize.y);

    vec2 uvw_upper = vec2(uv.x*s.x,(uv.y*s.y + 2.0*s.y)/3.0);
    vec2 uvw_lower = vec2(uv.x*s.x,(uv.y*s.y + s.y)/3.0);
    vec2 blendmask = vec2(uv.x*s.x,uv.y*s.y/3.0);

    vec3 n = texture2DRect(uvw_map,uvw_upper).xyz*2.0 - 1.0 + texture2DRect(uvw_map,uvw_lower).xyz*2.0/255.0 - 1.0 / 255.0;

    if (length(n) <= 0.01)
    {
        return;
    }
    uvw = normalize(n);

    vec2 texCoords;
    if (mapping(texCoords) < 0.0)
    {
        return;
    }

    if (map_mirror_horizontal)
    {
        texCoords.s = 1.0 - texCoords.s;
    }
    if (map_mirror_vertical)
    {
        texCoords.t = 1.0 - texCoords.t;
    }

    float _shift = s.y/3.0/4.0;
    cc_all.multiplier = 0.0;
    cc_green.multiplier = 0.0;
    cc_blue.multiplier = 0.0;
    float _off = s.y;
	
     getChannelCorrectionFromImage(uvw_map,s,uv,ALL_CHANNEL_SHIFT,cc_all);
     getChannelCorrectionFromImage(uvw_map,s,uv,RED_CHANNEL_SHIFT,cc_red);
     getChannelCorrectionFromImage(uvw_map,s,uv,GREEN_CHANNEL_SHIFT,cc_green);
     getChannelCorrectionFromImage(uvw_map,s,uv,BLUE_CHANNEL_SHIFT,cc_blue);

    float alpha =  texture2DRect(uvw_map,blendmask).r;
    vec3 color = colorCorrection(texture2DRect(image, texCoords * _image_imgSize).rgb);
    gl_FragColor = vec4(color*alpha,1.0);
}
