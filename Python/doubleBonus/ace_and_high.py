import numpy as np
import re
import sklearn.svm

pattern = '^.(..),(..),(..),(..),(..). (\\d+)$'

f = open('aceAndHigh.txt')
lines = map(lambda line: re.match(pattern, line), f.readlines())
f.close()

hands = [(tuple(line[i] for i in [1, 2, 3, 4, 5]), int(line[6]), line[0]) for line in lines]

# Cards are U, V, W, X, A for low cards in order, high card, Ace
# Input vector:
# [ 0]: U == 2
# [ 1]: U == 3
# [ 2]: U == 4
# [ 3]: U == 5
# [ 4]: U == 6
# [ 5]: U == 7
# [ 6]: U == 8
# [ 7]: V == 3
# [ 8]: V == 4
# [ 9]: V == 5
# [10]: V == 6
# [11]: V == 7
# [12]: V == 8
# [13]: V == 9
# [14]: W == 6
# [15]: W == 7
# [16]: W == 8
# [17]: W == 9
# [18]: W == 10
# [19]: X == 11
# [20]: X == 12
# [21]: X == 13
# [22]: U,V have the same suit
# [23]: V,W have the same suit
# [24]: U,W have the same suit
# [25]: U,X have the same suit
# [26]: V,X have the same suit
# [27]: W,X have the same suit
# [28]: U,A have the same suit
# [29]: V,A have the same suit
# [30]: W,A have the same suit
# Output vector:
# 1 if we should keep two cards, 0 if we only keep 1
def make_input_and_output(hand):
    x = np.zeros(31)
    suit = {'..23456789TJQKA'.find(card[0]): card[1] for card in hand[0]}
    U, V, W, X, A = sorted(suit.keys())
    x[U - 2] += 1
    x[V + 4] += 1
    x[W + 8] += 1
    suit_checks = [(U, V), (V, W), (U, W), (U, X), (V, X), (W, X), (U, A), (V, A), (W, A)]
    for i, (U, V) in enumerate(suit_checks):
        if suit[U] == suit[V]:
            x[22 + i] += 1
    y = bin(hand[1]).count('1') - 1
    return (x, y)

X, Y = zip(*map(make_input_and_output, hands))
X, Y = np.array(X), np.array(Y)

svm = sklearn.svm.LinearSVC(verbose=True)
svm.fit(X, Y)

print()

print(svm.score(X, Y))

def test_vector(v):
    pos = np.array([x for (x, y) in zip(X, Y) if y >= 0.5])
    neg = np.array([x for (x, y) in zip(X, Y) if y < 0.5])
    print('A w/ JQK: %f ~ %f' % (min(v.dot(pos.T)[0]), max(v.dot(pos.T)[0])))
    print('Ace only: %f ~ %f' % (min(v.dot(neg.T)[0]), max(v.dot(neg.T)[0])))

V = np.array([[
    0, 0, 0, 0, -2, -2, -1,
    0, 0, 0, 0, 0, 0, 0,
    0, 0, 0, 0, -1,
    0, 0, 0,
    0, 0, 0,
    0, 0, 0,
    1, 1, 2]])
