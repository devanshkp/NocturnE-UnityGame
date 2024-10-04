using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Smaple {
public class WerewolfControlScript : MonoBehaviour
{
    private Animator _Animator;
    private CharacterController _Ctrl;
    private Vector3 _MoveDirection = Vector3.zero;
    private GameObject _View_Camera;

    //----------------------
    // Start
    //----------------------
    void Start()
    {
        _Animator = this.GetComponent<Animator>();
        _Ctrl = this.GetComponent<CharacterController>();
        _View_Camera = GameObject.Find("Main Camera");
    }

    //----------------------
    // Update
    //----------------------
    void Update()
    {
        CAMERA();
        GRAVITY();
        STATUS();
        RESET_ANIMATION();

        if(!_Status.ContainsValue( true ))
        {
            MOVE();
            JUMP();
            DAMAGE();
            ATTACK();
            STOP();
            DOWN_FACING();
            CAUGHT();
            HANGED();
            SEARCH();
            SURPRISED();
            HOWL();
            ENERGY();
            LAUGH();
            LOOK_AROUND();
            DISGUISE();
        }
        else if(_Status.ContainsValue( true ))
        {
            string status_name = "";
            foreach(var i in _Status)
            {
                if(i.Value == true)
                {
                    status_name = i.Key;
                    break;
                }
            }
            if(status_name == "Jump")
            {
                MOVE();
                JUMP();
                STOP();
            }
            else if(status_name == "Damage")
            {
                DAMAGE();
            }
            else if(status_name == "Stop")
            {
                STOP();
            }
            else if(status_name == "Attack")
            {
                ATTACK();
            }
            else if(status_name == "Caught")
            {
                CAUGHT();
            }
            else if(status_name == "Hanged")
            {
                HANGED();
            }
            
        }
    }
    //--------------------------------------------------------------------- STATUS
    // Flags to control slime's action
    // It is used by method in Update()
    //---------------------------------------------------------------------
    private Dictionary<string, bool> _Status = new Dictionary<string, bool>
    {
        {"Jump", false },
        {"Damage", false },
        {"Stop", false },
        {"Attack", false },
        {"Caught", false},
        {"Hanged", false},
    };
    //------------------------------
    private void STATUS ()
    {
        if(_Animator.GetCurrentAnimatorStateInfo(0).IsTag("Jump"))
        {
            _Status["Jump"] = true;
        }
        else if(!_Animator.GetCurrentAnimatorStateInfo(0).IsTag("Jump"))
        {
            _Status["Jump"] = false;
        }

        if(_Animator.GetCurrentAnimatorStateInfo(0).IsTag("Damage"))
        {
            _Status["Damage"] = true;
        }
        else if(!_Animator.GetCurrentAnimatorStateInfo(0).IsTag("Damage"))
        {
            _Status["Damage"] = false;
        }

        if(_Animator.GetCurrentAnimatorStateInfo(0).IsTag("Stop"))
        {
            _Status["Stop"] = true;
        }
        else if(!_Animator.GetCurrentAnimatorStateInfo(0).IsTag("Stop"))
        {
            _Status["Stop"] = false;
        }

        if(_Animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack"))
        {
            _Status["Attack"] = true;
        }
        else if(!_Animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack"))
        {
            _Status["Attack"] = false;
        }

        if(_Animator.GetCurrentAnimatorStateInfo(0).IsTag("Caught"))
        {
            _Status["Caught"] = true;
        }
        else if(!_Animator.GetCurrentAnimatorStateInfo(0).IsTag("Caught"))
        {
            _Status["Caught"] = false;
        }

        if(_Animator.GetCurrentAnimatorStateInfo(0).IsTag("Hanged"))
        {
            _Status["Hanged"] = true;
        }
        else if(!_Animator.GetCurrentAnimatorStateInfo(0).IsTag("Hanged"))
        {
            _Status["Hanged"] = false;
        }
    }
    //---------------------------------------------------------------------
    // reset animation
    //---------------------------------------------------------------------
    private void RESET_ANIMATION ()
    {
        if(Input.GetKeyDown(KeyCode.R))
        {
            _Animator.CrossFade("idle", 0.1f, 0, 0);
        }
    }
    //--------------------------------------------------------------------- CAMERA
    // camera moving
    //---------------------------------------------------------------------
    private void CAMERA ()
	{
		_View_Camera.transform.position = this.transform.position + new Vector3(0, 0.5f, 2.0f);
	}
    //--------------------------------------------------------------------- GRAVITY
    // gravity for fall of slime
    //---------------------------------------------------------------------
	private void GRAVITY ()
	{
		if (_Ctrl.isGrounded)
		{
			if(_MoveDirection.y < -0.1f){
                _MoveDirection.y = -0.1f;
            }
		}
		else if (!_Ctrl.isGrounded){
            _MoveDirection.y -= 0.5f;
        }
		_Ctrl.Move(_MoveDirection * Time.deltaTime);
	}
    //--------------------------------------------------------------------- isGrounded
    // whether it is grounded
    //---------------------------------------------------------------------
    private bool CheckGrounded()
    {
        if (_Ctrl.isGrounded){
            return true;
        }
        Ray ray = new Ray(this.transform.position + Vector3.up * 0.1f, Vector3.down);
        float range = 0.3f;
        return Physics.Raycast(ray, range);
    }
    //--------------------------------------------------------------------- MOVE
    // for slime moving
    //---------------------------------------------------------------------
	private void MOVE ()
    {
        float speed = _Animator.GetFloat("Speed") + 1;
		//------------------------------------------------------------ Speed
        if(Input.GetKey(KeyCode.Z))
        {
            if(speed <= 2){
                speed += 0.01f;
            }
            else if(speed >= 2){
                speed = 2;
            }
        }
	    else {
            if(speed >= 1){
                speed -= 0.01f;
            }
            else if(speed <= 1){
                speed = 1;
            }
        }
        _Animator.SetFloat("Speed", speed - 1);

        //------------------------------------------------------------ Foreward
        if (Input.GetKey(KeyCode.UpArrow))
        {
            // velocity
            if(_Animator.GetCurrentAnimatorStateInfo(0).IsName("move") || !CheckGrounded())
            {
                Vector3 velocity = this.transform.rotation * new Vector3(0, 0, speed);
                MOVE_XZ(velocity);
                MOVE_RESET();
            }
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (CheckGrounded())
		    {
                if(!_Animator.GetCurrentAnimatorStateInfo(0).IsName("jump")){
                    _Animator.CrossFade("move", 0.1f, 0, 0);
                }
            }
        }
        
        //------------------------------------------------------------ character rotation
        if (Input.GetKey(KeyCode.RightArrow) && !Input.GetKey(KeyCode.LeftArrow)){
            this.transform.Rotate(Vector3.up, 2.5f);
        }
        else if (Input.GetKey(KeyCode.LeftArrow) && !Input.GetKey(KeyCode.RightArrow)){
            this.transform.Rotate(Vector3.up, -2.5f);
        }
        if (!Input.GetKey(KeyCode.UpArrow) && !Input.GetKey(KeyCode.DownArrow))
        {
            if(!_Animator.GetCurrentAnimatorStateInfo(0).IsName("jump"))
            {
                if (Input.GetKeyDown(KeyCode.RightArrow) && !Input.GetKey(KeyCode.LeftArrow)){
            	    _Animator.CrossFade("move", 0.1f, 0, 0);
                }
                else if (Input.GetKeyDown(KeyCode.LeftArrow) && !Input.GetKey(KeyCode.RightArrow)){
            	    _Animator.CrossFade("move", 0.1f, 0, 0);
                }
            }
            // rotate stop
            else if (Input.GetKey(KeyCode.RightArrow) && Input.GetKey(KeyCode.LeftArrow))
            {
                if(!_Animator.GetCurrentAnimatorStateInfo(0).IsName("jump")){
            	    _Animator.CrossFade("idle", 0.1f, 0, 0);
                }
            }
        }
        KEY_UP();
	}
    //--------------------------------------------------------------------- KEY_UP
    // whether arrow key is key up
    //---------------------------------------------------------------------
	private void KEY_UP ()
	{
	    if (Input.GetKeyUp(KeyCode.UpArrow))
        {
            if(!Input.GetKey(KeyCode.LeftArrow) && !Input.GetKey(KeyCode.RightArrow)){
                _Animator.CrossFade("idle", 0.1f, 0, 0);
            }
        }
        else if (!Input.GetKey(KeyCode.UpArrow) && !Input.GetKey(KeyCode.DownArrow))
        {
        	if (Input.GetKeyUp(KeyCode.RightArrow) || Input.GetKeyUp(KeyCode.LeftArrow))
            {
                if(Input.GetKey(KeyCode.LeftArrow)){
                    _Animator.CrossFade("move", 0.1f, 0, 0);
                }
                else if(Input.GetKey(KeyCode.RightArrow)){
                    _Animator.CrossFade("move", 0.1f, 0, 0);
                }
                else{
            		_Animator.CrossFade("idle", 0.1f, 0, 0);
                }
            }
        }
	}
    //--------------------------------------------------------------------- MOVE_SUB
    // value for moving
    //---------------------------------------------------------------------
	private void MOVE_XZ (Vector3 velocity)
	{
        _MoveDirection = new Vector3 (velocity.x, _MoveDirection.y, velocity.z);
        _Ctrl.Move(_MoveDirection * Time.deltaTime);
    }
    private void MOVE_RESET()
    {
        _MoveDirection.x = 0;
        _MoveDirection.z = 0;
    }
    //--------------------------------------------------------------------- JUMP
    // for jumping
    //---------------------------------------------------------------------
	private void JUMP ()
	{
        if(CheckGrounded())
        {
		    if(Input.GetKeyDown(KeyCode.S)
                && !_Animator.IsInTransition(0))
		    {
                _Animator.CrossFade("jump", 0.1f, 0, 0);
                // jump power
                _MoveDirection.y = 5.0f;
			}
        }
        if (_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1
            && _Animator.GetCurrentAnimatorStateInfo(0).IsName("jump")
            && !_Animator.IsInTransition(0))
        {
            if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow)
                || Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow))
            {
                _Animator.CrossFade("move", 0.1f, 0, 0);
            }
            else{
                _Animator.CrossFade("idle", 0.1f, 0, 0);
            }
        }
	}
    //--------------------------------------------------------------------- DAMAGE
    // play animation of damage
    //---------------------------------------------------------------------
	private void DAMAGE ()
	{
		if (Input.GetKeyDown(KeyCode.Q))
		{
			_Animator.CrossFade("damage", 0.1f, 0, 0);
		}
        if (_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1
            && _Animator.GetCurrentAnimatorStateInfo(0).IsTag("Damage")
            && !_Animator.IsInTransition(0))
        {
            _Animator.CrossFade("idle", 0.1f, 0, 0);
        }
	}
    //--------------------------------------------------------------------- ATTACK
    // play animation of attack
    //---------------------------------------------------------------------
    private void ATTACK ()
    {
        if (Input.GetKeyDown(KeyCode.A))
	    {
	    	_Animator.CrossFade("attack1_charge", 0.1f, 0, 0);
		}
        if (_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f
            && _Animator.GetCurrentAnimatorStateInfo(0).IsName("attack1_charge")
            && !_Animator.IsInTransition(0))
        {
            _Animator.CrossFade("attack1", 0.1f, 0, 0);
        }
        if (_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f
            && _Animator.GetCurrentAnimatorStateInfo(0).IsName("attack1")
            && !_Animator.IsInTransition(0))
        {
            _Animator.CrossFade("attack2_charge", 0.1f, 0, 0);
        }
        if (_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f
            && _Animator.GetCurrentAnimatorStateInfo(0).IsName("attack2_charge")
            && !_Animator.IsInTransition(0))
        {
            _Animator.CrossFade("attack2", 0.1f, 0, 0);
        }
        if (_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f
            && _Animator.GetCurrentAnimatorStateInfo(0).IsName("attack2")
            && !_Animator.IsInTransition(0))
        {
            _Animator.CrossFade("attack3_charge", 0.1f, 0, 0);
        }
        if (_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f
            && _Animator.GetCurrentAnimatorStateInfo(0).IsName("attack3_charge")
            && !_Animator.IsInTransition(0))
        {
            _Animator.CrossFade("attack3", 0.1f, 0, 0);
        }

        if(_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f
            && _Animator.GetCurrentAnimatorStateInfo(0).IsName("attack3")
            && !_Animator.IsInTransition(0))
        {
            _Animator.CrossFade("idle", 0.1f, 0, 0);
        }
    }

    //--------------------------------------------------------------------- STOP
    // play animation of down and jump of resurrection
    //---------------------------------------------------------------------
    private void STOP ()
    {
        if (Input.GetKeyDown(KeyCode.E)
            && !_Animator.GetCurrentAnimatorStateInfo(0).IsTag("Stop"))
	    {
            _Animator.CrossFade("down", 0.1f, 0, 0);
        }
        if (Input.GetKeyDown(KeyCode.E)
            && _Animator.GetCurrentAnimatorStateInfo(0).IsTag("Stop")
            && !_Animator.IsInTransition(0))
	    {
            _Animator.CrossFade("jump", 0.1f, 0, 0);
        }
    }
    // value for facing of down animation
    private void DOWN_FACING ()
    {
        if(Input.GetKey(KeyCode.UpArrow))
        {
            _Animator.SetFloat("DownFacing", 1);
        }
        else
        {
            _Animator.SetFloat("DownFacing", 0);
        }
    }
    //--------------------------------------------------------------------- CAUGHT
    // play animation of caught
    //---------------------------------------------------------------------
    private void CAUGHT ()
    {
        if (Input.GetKeyDown(KeyCode.J))
	    {
	    	_Animator.CrossFade("caught", 0.1f, 0, 0);
		}
        if (_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f
            && _Animator.GetCurrentAnimatorStateInfo(0).IsName("caught")
            && !_Animator.IsInTransition(0))
        {
            _Animator.CrossFade("caught_down", 0.1f, 0, 0);
        }
        if(_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f
            && _Animator.GetCurrentAnimatorStateInfo(0).IsName("caught_down")
            && !_Animator.IsInTransition(0))
        {
            _Animator.CrossFade("caught_faint", 0.1f, 0, 0);
        }
        if(_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f
            && _Animator.GetCurrentAnimatorStateInfo(0).IsName("caught_faint")
            && !_Animator.IsInTransition(0))
        {
            _Animator.CrossFade("idle", 0.1f, 0, 0);
        }
    }
    //--------------------------------------------------------------------- HANGED
    // play animation of hanged
    //---------------------------------------------------------------------
    private void HANGED ()
    {
        if (Input.GetKeyDown(KeyCode.H))
	    {
	    	_Animator.CrossFade("hanged", 0.1f, 0, 0);
		}
        if(_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f
            && _Animator.GetCurrentAnimatorStateInfo(0).IsName("hanged")
            && !_Animator.IsInTransition(0))
        {
            _Animator.CrossFade("idle", 0.1f, 0, 0);
        }
    }
    //--------------------------------------------------------------------- SEARCH
    // play animation of search
    //---------------------------------------------------------------------
    private void SEARCH ()
    {
        if (Input.GetKeyDown(KeyCode.X))
	    {
	    	_Animator.CrossFade("search", 0.1f, 0, 0);
		}
        if(_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f
            && _Animator.GetCurrentAnimatorStateInfo(0).IsName("search")
            && !_Animator.IsInTransition(0))
        {
            _Animator.CrossFade("idle", 0.1f, 0, 0);
        }
    }
    //--------------------------------------------------------------------- SURPRISED
    // play animation of surprised
    //---------------------------------------------------------------------
    private void SURPRISED ()
    {
        if (Input.GetKeyDown(KeyCode.W))
	    {
	    	_Animator.CrossFade("surprised", 0.1f, 0, 0);
		}
        if(_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f
            && _Animator.GetCurrentAnimatorStateInfo(0).IsName("surprised")
            && !_Animator.IsInTransition(0))
        {
            _Animator.CrossFade("idle", 0.1f, 0, 0);
        }
    }
    //--------------------------------------------------------------------- HOWL
    // play animation of howl
    //---------------------------------------------------------------------
    private void HOWL ()
    {
        if (Input.GetKeyDown(KeyCode.C))
	    {
	    	_Animator.CrossFade("howl", 0.3f, 0, 0);
		}
        if(_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f
            && _Animator.GetCurrentAnimatorStateInfo(0).IsName("howl")
            && !_Animator.IsInTransition(0))
        {
            _Animator.CrossFade("howling", 0.1f, 0, 0);
        }
        if(_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f
            && _Animator.GetCurrentAnimatorStateInfo(0).IsName("howling")
            && !_Animator.IsInTransition(0))
        {
            _Animator.CrossFade("idle", 0.1f, 0, 0);
        }
    }
    //--------------------------------------------------------------------- ENERGY
    // play animation of energy
    //---------------------------------------------------------------------
    private void ENERGY ()
    {
        if (Input.GetKeyDown(KeyCode.Y))
	    {
	    	_Animator.CrossFade("energy", 0.1f, 0, 0);
		}
        if(_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 2.0f
            && _Animator.GetCurrentAnimatorStateInfo(0).IsName("energy")
            && !_Animator.IsInTransition(0))
        {
            _Animator.CrossFade("idle", 0.1f, 0, 0);
        }
    }
    //--------------------------------------------------------------------- LAUGH
    // play animation of laugh
    //---------------------------------------------------------------------
    private void LAUGH ()
    {
        if (Input.GetKeyDown(KeyCode.U))
	    {
	    	_Animator.CrossFade("laugh", 0.1f, 0, 0);
		}
        if(_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 2.0f
            && _Animator.GetCurrentAnimatorStateInfo(0).IsName("laugh")
            && !_Animator.IsInTransition(0))
        {
            _Animator.CrossFade("idle", 0.1f, 0, 0);
        }
    }
    //--------------------------------------------------------------------- LOOK_AROUND
    // play animation of look around
    //---------------------------------------------------------------------
    private void LOOK_AROUND ()
    {
        if (Input.GetKeyDown(KeyCode.I))
	    {
	    	_Animator.CrossFade("lookaround", 0.1f, 0, 0);
		}
        if(_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.8f
            && _Animator.GetCurrentAnimatorStateInfo(0).IsName("lookaround")
            && !_Animator.IsInTransition(0))
        {
            _Animator.CrossFade("idle", 0.1f, 0, 0);
        }
    }
    //--------------------------------------------------------------------- DISGUISE
    // play animation of disguise
    //---------------------------------------------------------------------
    private void DISGUISE ()
    {
        if (Input.GetKeyDown(KeyCode.K))
	    {
	    	_Animator.CrossFade("disguise", 0.1f, 0, 0);
		}
        if(_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f
            && _Animator.GetCurrentAnimatorStateInfo(0).IsName("disguise")
            && !_Animator.IsInTransition(0))
        {
            _Animator.CrossFade("idle", 0.3f, 0, 0);
        }
    }
}
}