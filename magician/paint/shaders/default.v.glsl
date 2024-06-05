#version 330 core

layout (location = 0) in vec3 pos;
layout (location = 1) in vec4 rgba;

out vec4 outRgba;

uniform mat4 view;
uniform mat4 proj;

void main()
{
    gl_Position = proj*view*vec4(pos, 1.0);
    outRgba = rgba;
}