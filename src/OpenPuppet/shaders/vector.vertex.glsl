#version 330 core

layout (location = 0) in vec3 vPosition;
layout (location = 1) in vec4 vColor;

uniform mat4 proj;
uniform mat4 view;

out vec4 fColor;

void main() {
    gl_Position =  proj * view * vec4(vPosition, 1.0);
    fColor = vColor;
}