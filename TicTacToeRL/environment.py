import time
from threading import Thread

import tensorflow as tf
import numpy as np
from tf_agents.environments import py_environment

from tf_agents.environments import utils
from tf_agents.specs import array_spec
from tf_agents.environments import wrappers
from tf_agents.environments import suite_gym
from tf_agents.trajectories import time_step as ts

class Environment(py_environment.PyEnvironment):
    def __init__(self):
        self._action_spec = array_spec.BoundedArraySpec(shape=(), dtype=np.int32, minimum=0, maximum=8, name='play')

        self._observation_spec = array_spec.BoundedArraySpec(shape = (1,9), dtype=np.int32, minimum=0, maximum=2, name='board')
        self._state = [0, 0, 0, 0, 0, 0, 0, 0, 0]
        self._episode_ended = False
        self._computer_victory = 0
        self._player_victory = 0
        self._invalid_moves = 0
        self._ties = 0
        self._true_start = False
        self._current_agent = 0

    def episode_ended(self):
        return self._episode_ended

    def action_spec(self):
        return self._action_spec

    def observation_spec(self):
        return self._observation_spec

    def _reset(self):
        self._state = [0, 0, 0, 0, 0, 0, 0, 0, 0]
        self._episode_ended = False
        self._player_turn = True
        return ts.restart(np.array([self._state], dtype=np.int32))

    def __is_spot_empty(self, ind):
        return self._state[ind] == 0

    def __all_spots_occupied(self):
        return all(i > 0 for i in self._state)

    def _check_for_victory(self, player):
        if self._state[0] == player and self._state[1] == player and self._state[2] == player:
            return True
        if self._state[0] == player and self._state[3] == player and self._state[6] == player:
            return True
        if self._state[0] == player and self._state[4] == player and self._state[8] == player:
            return True
        if self._state[3] == player and self._state[4] == player and self._state[5] == player:
            return True
        if self._state[1] == player and self._state[4] == player and self._state[7] == player:
            return True
        if self._state[2] == player and self._state[4] == player and self._state[6] == player:
            return True
        if self._state[6] == player and self._state[7] == player and self._state[8] == player:
            return True
        if self._state[2] == player and self._state[5] == player and self._state[8] == player:
            return True

        return False

    def _step(self, action):
        # print(action)
        if self._episode_ended:
            return self.reset()

        if self.__all_spots_occupied():
            self._episode_ended = True
            print("Tie!")
            self._ties += 1
            return ts.termination(np.array([self._state], dtype=np.int32), 0.5)

        #Check for player victory
        if self._check_for_victory(2):
            self._episode_ended = True
            print("Player victory!")
            self._player_victory += 1
            if self._current_agent == 0:
                return ts.termination(np.array([self._state], dtype=np.int32), -1)
            else:
                return ts.termination(np.array([self._state], dtype=np.int32), 1)

        if self.__is_spot_empty(action):
            if self._current_agent == 0:
                self._state[action] = 1
            else:
                self._state[action] = 2

            # Check for computer victory
            if self._check_for_victory(1):
                self._episode_ended = True
                print("Computer Victory!")
                self._computer_victory += 1
                if self._current_agent == 0:
                    return ts.termination(np.array([self._state], dtype=np.int32), 1)
                else:
                    return

            if self.__all_spots_occupied():
                self._episode_ended = True
                print("Tie!")
                self._ties += 1
                return ts.termination(np.array([self._state], dtype=np.int32), 0.5)
            else:
                return ts.transition(np.array([self._state], dtype=np.int32), reward=0.05, discount=1.0)
        else:
            self._episode_ended = True
            print("invalid move!")
            self._invalid_moves += 1
            # if self._true_start:
            #     print(self._state)
            #     print(action)
            #     time.sleep(5)
            #raise Exception("Invalid Move made")
            return ts.termination(np.array([self._state], dtype=np.int32), -10)