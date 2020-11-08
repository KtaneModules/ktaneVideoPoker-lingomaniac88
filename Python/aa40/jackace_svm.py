import numpy as np
import re
import sklearn.svm

pattern = 'Hand=.(..),(..),(..),(..),(..). Optimal=.(\\d+).'

f = open('jackace.txt')
lines = map(lambda line: re.match(pattern, line), f.readlines())
f.close()

hands = [(tuple(line[i] for i in [1, 2, 3, 4, 5]), int(line[6]), line[0]) for line in lines]

# Cards are L, M, H, J, A for low, middle, high, Jack, Ace
# Input vector:
# [ 0]: L == 2
# [ 1]: L == 3
# [ 2]: L == 4
# [ 3]: L == 5
# [ 4]: L == 6
# [ 5]: M == 3
# [ 6]: M == 4
# [ 7]: M == 5
# [ 8]: M == 6
# [ 9]: M == 7
# [10]: M == 8
# [11]: H == 6
# [12]: H == 7
# [13]: H == 8
# [14]: H == 9
# [15]: H == 10
# [16]: L,M have the same suit
# [17]: M,H have the same suit
# [18]: L,H have the same suit
# [19]: L,J have the same suit
# [20]: M,J have the same suit
# [21]: H,J have the same suit
# [22]: L,A have the same suit
# [23]: M,A have the same suit
# [24]: H,A have the same suit
# Output vector:
# 1 if we should keep the Ace, 0 otherwise
def make_input_and_output(hand):
    x = np.zeros(25)
    suit = {'..23456789TJQKA'.find(card[0]): card[1] for card in hand[0]}
    L, M, H, J, A = sorted(suit.keys())
    x[L - 2] += 1
    x[M + 2] += 1
    x[H + 5] += 1
    suit_checks = [(L, M), (M, H), (L, H), (L, J), (M, J), (H, J), (L, A), (M, A), (H, A)]
    for i, (U, V) in enumerate(suit_checks):
        if suit[U] == suit[V]:
            x[16 + i] += 1
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
    print('J A: %f ~ %f' % (min(v.dot(pos.T)[0]), max(v.dot(pos.T)[0])))
    print(' J : %f ~ %f' % (min(v.dot(neg.T)[0]), max(v.dot(neg.T)[0])))

V = np.array([[-1, -1, -1, -1, -1,
               0, 0, 0, 0, 0, 0,
               -17, 0, 0, 15, 0,
               0, 0, 0,
               16, 16, 16,
               0, 0, 0]])

Z = np.array([[0, 0, 0, 0, 0,
               0, 0, 0, 0, 0, 0,
               -1, 0, 0, 1, 0,
               0, 0, 0,
               1, 1, 1,
               0, 0, 0]])
