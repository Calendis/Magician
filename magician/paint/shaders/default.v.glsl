#version 330 core

layout (location = 0) in vec3 pos;
layout (location = 1) in vec4 rgba;
out vec3 pos2;
out vec4 rgba2;

void main()
{
    gl_Position = vec4(pos, 1.0);
    pos2 = pos;
    rgba2 = rgba;
}