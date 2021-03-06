﻿/*
 * Copyright (C) 2009, Google Inc.
 * Copyright (C) 2009, Gil Ran <gilrun@gmail.com>
 *
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or
 * without modification, are permitted provided that the following
 * conditions are met:
 *
 * - Redistributions of source code must retain the above copyright
 *   notice, this list of conditions and the following disclaimer.
 *
 * - Redistributions in binary form must reproduce the above
 *   copyright notice, this list of conditions and the following
 *   disclaimer in the documentation and/or other materials provided
 *   with the distribution.
 *
 * - Neither the name of the Git Development Community nor the
 *   names of its contributors may be used to endorse or promote
 *   products derived from this software without specific prior
 *   written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND
 * CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES,
 * INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
 * NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
 * CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
 * STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections;
using System.Collections.Generic;

namespace GitSharp.Diff
{
    /** Specialized list of {@link Edit}s in a document. */
    public class EditList : ArrayList
    {
	    public int size()
        {
		    return this.Count;
        }

	    public Edit get(int index)
        {
            return (Edit)this[index];
	    }

	    public Edit set(int index, Edit element)
        {
            Edit retval = (Edit)this[index];
            this[index] = element;
            return retval;
	    }

	    public void Add(int index, Edit element)
        {
		    this.Insert(index, element);
	    }

	    public void remove(int index)
        {
		    this.RemoveAt(index);
	    }

	    public int hashCode()
        {
		    return this.GetHashCode();
	    }

	    public override String ToString()
        {
            /* Unfortunately, C#'s List does not implement ToString the same
             * way Java's ArrayList does. It simply inherits from the base class
             * object. This means that ToString returns the string identifier of
             * the type.
             * Until a better solution is found, I'm implementing ToString myself.
             */
            string retval = "EditList[";
            foreach (Edit e in this)
                retval = retval + e.ToString();
            retval = retval + "]";
            return retval;
	    }

        /* This method did not exist in the original Java code.
         * In Java, the AbstractList has a method named isEmpty
         * C#'s AbstractList has no such method
         */
        public bool isEmpty()
        {
            return (this.Count == 0);
        }

        private bool isEqual(EditList o)
        {
            if (this.Count != ((EditList)o).Count)
                return false;

            for (int i = 0; i < this.Count; i++)
                if (!this[i].Equals(((EditList)o)[i]))
                    return false;

            return true;
        }

        private bool isEqual(string s)
        {
            return this.ToString().Equals(s);
        }

        public override bool Equals(object o)
        {
            if (o is EditList)
                return this.isEqual((EditList)o);

            if (o is string)
                return this.isEqual((string)o);

            return false;
        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }
    }
}