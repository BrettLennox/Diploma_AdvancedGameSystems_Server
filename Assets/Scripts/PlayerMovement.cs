using System;
using System.Collections;
using System.Collections.Generic;
using RiptideNetworking;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Player _player;
    [SerializeField] private CharacterController _controller;
    [SerializeField] private Transform _camProxy;
    [SerializeField] private float _gravity;
    [SerializeField] private float _movementSpeed;
    [SerializeField] private float _jumpHeight;

    private float _gravityAcceleration;
    private float _moveSpeed;
    private float _jumpSpeed;

    private bool[] _inputs;
    private float _yVelocity;

    private void OnValidate()
    {
        if (_controller == null)
            _controller = GetComponent<CharacterController>();
        if (_player == null)
            _player = GetComponent<Player>();

        Initialize();
    }

    private void Start()
    {
        Initialize();
        
        _inputs = new bool[6];
    }

    private void FixedUpdate()
    {
        Vector2 inputDirections = Vector2.zero;
        if (_inputs[0])
            inputDirections.y += 1;
        
        if (_inputs[1])
            inputDirections.y -= 1;
        
        if (_inputs[2])
            inputDirections.x += 1;
        
        if (_inputs[3])
            inputDirections.x -= 1;
        
        Move(inputDirections, _inputs[4], _inputs[5]);
    }

    private void Initialize()
    {
        _gravityAcceleration = _gravity * Time.fixedDeltaTime * Time.fixedDeltaTime;
        _moveSpeed = _movementSpeed * Time.fixedDeltaTime;
        _jumpSpeed = Mathf.Sqrt(_jumpHeight * -2f * _gravityAcceleration);
    }

    private void Move(Vector2 inputDirection, bool jump, bool sprint)
    {
        Vector3 moveDirection = Vector3.Normalize(_camProxy.right * inputDirection.x + Vector3.Normalize(FlattenVector3(_camProxy.forward)) * inputDirection.y);
        moveDirection *= _moveSpeed;

        if (sprint)
            moveDirection *= 2f;

        if (_controller.isGrounded)
        {
            _yVelocity = 0f;
            if (jump)
            {
                _yVelocity = _jumpSpeed;
            }
        }

        _yVelocity += _gravityAcceleration;

        moveDirection.y = _yVelocity;
        _controller.Move((moveDirection));

        SendMovement();
    }

    private Vector3 FlattenVector3(Vector3 vector)
    {
        vector.y = 0;
        return vector;
    }

    public void SetInput(bool[] inputs, Vector3 forward)
    {
        this._inputs = inputs;
        _camProxy.forward = Vector3.forward;
    }

    private void SendMovement()
    {
        if (NetworkManager.NetworkManagerInstance.CurrentTick % 2 != 0)
            return;
        Message message = Message.Create(MessageSendMode.unreliable, ServerToClientId.playerMovement);
        message.AddUShort(_player.Id);
        message.AddUShort(NetworkManager.NetworkManagerInstance.CurrentTick);
        message.AddVector3(transform.position);
        message.AddVector3(_camProxy.forward);
        NetworkManager.NetworkManagerInstance.GameServer.SendToAll(message);
    }
}
